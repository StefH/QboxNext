using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using QboxNext.Logging;

namespace QboxNext.Simulation
{
    /// <summary>
    /// PatternParser is used to parse the pattern parameter of the command line.
    /// </summary>
    public static class PatternParser
    {
        private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger("PatternParser");

        /// <summary>
        /// Parse pattern parameter of the form "1:flat(2);2:block_half(3,30,15);3:block_half(2,30,0)" into a list of pattern specs.
        /// </summary>
        public static List<UsagePatternSpec> ParseSpecList(string inPatternSpec)
        {
            if (string.IsNullOrEmpty(inPatternSpec))
                return new List<UsagePatternSpec>();

            var parts = inPatternSpec.Split(';');
            var patternSpecs = new List<UsagePatternSpec>(parts.Count());

            foreach (var part in parts)
            {
                var spec = ParseSpec(part);
                if (spec == null)
                    Logger.LogError("Error: could not parse pattern {0}", part);
                else
                    patternSpecs.Add(spec);
            }

            return patternSpecs;
        }


        /// <summary>
        /// Parse one part of a pattern parameter.
        /// </summary>
        /// <returns>A filled out pattern spec on success, null otherwise.</returns>
        public static UsagePatternSpec ParseSpec(string inPatternPart)
        {
            if (string.IsNullOrEmpty(inPatternPart))
                return null;

            var spec = new UsagePatternSpec();
            var trimmedPart = inPatternPart.Trim(';');

            var colonPos = trimmedPart.IndexOf(':');
            if (colonPos < 0)
                return null;

            var counterIdStr = trimmedPart.Substring(0, colonPos);
            int counterId;
            if (!int.TryParse(counterIdStr, out counterId))
                return null;
            spec.CounterId = counterId;

            var parenthesesOpenPos = trimmedPart.IndexOf('(', colonPos + 1);
            var pos = parenthesesOpenPos > 0 ? parenthesesOpenPos : trimmedPart.Length;
            var shape = trimmedPart.Substring(colonPos + 1, pos - colonPos - 1);
            spec.Shape = ParseShapeString(shape);

            if (parenthesesOpenPos > 0)
            {
                var parenthesesClosePos = trimmedPart.IndexOf(')');
                if (parenthesesClosePos < 0)
                    return null;

                var paramsString = trimmedPart.Substring(parenthesesOpenPos + 1, parenthesesClosePos - parenthesesOpenPos - 1);
                var paramStrings = GetParamStrings(paramsString);
                if (paramStrings.Count > 0)
                {
                    float value;
                    if (!float.TryParse(paramStrings[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
                        return null;
                    spec.Scale = value;
                }
                if (paramStrings.Count > 1)
                {
                    uint value;
                    if (!uint.TryParse(paramStrings[1], out value))
                        return null;
                    spec.Period = value;
                    // An odd number of measurements in a period will lead to strange behaviour, so enforce an even number.
                    if (spec.Period % 2 != 0)
                        Logger.LogError("Error: only an even number of measurements in a period is allowed.");
                }
                if (paramStrings.Count > 2)
                {
                    uint value;
                    if (!uint.TryParse(paramStrings[2], out value))
                        return null;
                    spec.SequenceOffset = value;
                }
                if (paramStrings.Count > 3)
                {
                    uint value;
                    if (!uint.TryParse(paramStrings[3], out value))
                        return null;
                    spec.CounterOffset = value;
                }
            }

            return spec;
        }


        /// <summary>
        /// Convert a string to a EUsageShape.
        /// </summary>
        private static EUsageShape ParseShapeString(string inShapeString)
        {
            switch (inShapeString)
            {
                case "":
                    return EUsageShape.None;
                case "zero":
                    return EUsageShape.Zero;
                case "zero_peak":
                    return EUsageShape.ZeroPeak;
                case "flat":
                    return EUsageShape.Flat;
                case "flat_peak":
                    return EUsageShape.FlatPeak;
                case "block":
                    return EUsageShape.Block;
                case "block_half":
                    return EUsageShape.BlockHalf;
                case "sine":
                    return EUsageShape.Sine;
                case "random":
                    return EUsageShape.Random;
                default:
                    Logger.LogError("Error: unknown shape {0}", inShapeString);
                    return EUsageShape.None;
            }
        }


        /// <summary>
        /// Convert "a, b, c" to ["a", "b", "c"].
        /// </summary>
        private static List<string> GetParamStrings(string inParamsString)
        {
            var paramsParts = inParamsString.Split(',');
            var trimmedParams = new List<string>(paramsParts.Count());
            trimmedParams.AddRange(paramsParts.Select(paramsPart => paramsPart.Trim(',').Trim()));
            return trimmedParams;
        }
    }
}
