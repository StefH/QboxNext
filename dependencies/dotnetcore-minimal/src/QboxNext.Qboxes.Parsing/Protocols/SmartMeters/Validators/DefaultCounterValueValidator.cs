namespace QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators
{
    /// <summary>
    /// The default validator.
    /// </summary>
    internal class DefaultCounterValueValidator : ICounterValueValidator
    {
        /// <inheritdoc />
        public bool CanValidate(int counterId)
        {
            return true;
        }

        /// <inheritdoc />
        public bool Validate(int digits, int precision)
        {
            return digits == 5 && precision == 3;
        }

        /// <inheritdoc />
        public string FormatError(string parsedValue, int counterId)
        {
            return $"Value {parsedValue} not in correct format for counter {counterId}, #####.###";
        }
    }
}