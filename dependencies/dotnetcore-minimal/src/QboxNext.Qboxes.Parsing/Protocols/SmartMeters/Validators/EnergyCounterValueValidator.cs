namespace QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators
{
    internal class EnergyCounterValueValidator : ICounterValueValidator
    {
        /// <inheritdoc />
        public bool CanValidate(int counterId)
        {
            return counterId == 181 || counterId == 182 || counterId == 281 || counterId == 282;
        }

        /// <inheritdoc />
        public bool Validate(int digits, int precision)
        {
            return digits == 7 && precision == 1
                   || digits == 6 && precision == 2
                   || digits >= 5 && precision == 3;
        }

        /// <inheritdoc />
        public string FormatError(string parsedValue, int counterId)
        {
            return $"Value {parsedValue} not in correct format for counter {counterId}, #######.# or ######.## or #####.### or ######.###";
        }
    }
}