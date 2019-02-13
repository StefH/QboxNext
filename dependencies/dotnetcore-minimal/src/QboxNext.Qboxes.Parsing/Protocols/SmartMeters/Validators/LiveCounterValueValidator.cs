namespace QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators
{
    internal class LiveCounterValueValidator : ICounterValueValidator
    {
        /// <inheritdoc />
        public bool CanValidate(int counterId)
        {
            return counterId == 170 || counterId == 270;
        }

        /// <inheritdoc />
        public bool Validate(int digits, int precision)
        {
            return digits == 5 && precision == 1
                   || digits == 4 && precision == 2
                   || digits == 2 && precision == 3
                   || digits == 5 && precision == 2;
        }

        /// <inheritdoc />
        public string FormatError(string parsedValue, int counterId)
        {
            return $"Value {parsedValue} not in correct format for counter {counterId}, #####.# or ####.## or ##.### or #####.##";
        }
    }
}