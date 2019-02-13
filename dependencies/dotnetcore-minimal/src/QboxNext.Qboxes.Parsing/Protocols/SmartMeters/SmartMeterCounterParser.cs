using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators;

namespace QboxNext.Qboxes.Parsing.Protocols.SmartMeters
{
    public class SmartMeterCounterParser
    {
        private readonly ILogger<SmartMeterCounterParser> _logger;
        private readonly ICollection<ICounterValueValidator> _validators;

        public SmartMeterCounterParser(ILogger<SmartMeterCounterParser> logger, IEnumerable<ICounterValueValidator> validators)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validators = validators?.ToList() ?? throw new ArgumentNullException(nameof(validators));
        }

        /// <summary>
        /// Parse the counter value from a SmartMeter part.
        /// </summary>
        /// <param name="value">
        /// String containing one SmartMeter value, for example "1.8.1(00214.037*kWh) 1-0".
        /// Note that the last "1-0" is actually part of the next counter value, so it will not always be present.
        /// </param>
        /// <param name="counter"></param>
        /// <returns>
        /// The value of the counter * 1000 if successful, otherwise MaxInt64.
        /// This means that for elecricity, the returned unit is Wh, for gas the unit is liter.
        /// </returns>
        public ulong Parse(string value, int counter)
        {
            _logger.LogTrace("Enter - value: {value}", value);
            ulong result = ulong.MaxValue;

            if (!string.IsNullOrEmpty(value))
            {
                int offset = value.LastIndexOf('(') + 1;
                int length = (value.IndexOf('*') == -1 ? value.LastIndexOf(')') : value.IndexOf('*')) - offset;
                int separator = value.IndexOf('.', offset);
                if (offset >= 0 && length > 0)
                {
                    int digits = separator == -1 ? length : separator - offset;
                    int precision = separator == -1 ? 0 : length - digits - 1;
                    string parsedValue = value.Substring(offset, length);

                    ICounterValueValidator validator = _validators.SingleOrDefault(v => v.CanValidate(counter)) ?? new DefaultCounterValueValidator();
                    if (!validator.Validate(digits, precision))
                    {
                        throw new SmartMeterProtocolException(validator.FormatError(parsedValue, counter));
                    }

                    decimal partialResult = decimal.Parse(parsedValue, CultureInfo.InvariantCulture);
                    result = Convert.ToUInt64(partialResult * 1000m);
                }
            }

            _logger.LogTrace("Return - {result}", result);
            return result;
        }
    }
}