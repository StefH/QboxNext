namespace QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators
{
    /// <summary>
    /// Describes a validator for a smart meter counter value.
    /// </summary>
    public interface ICounterValueValidator
    {
        /// <summary>
        /// Checks if the validator can validate the counter.
        /// </summary>
        /// <param name="counterId">The counter id.</param>
        /// <returns><see langword="true" /> if the counter can be validated by this validator, or <see langword="false" /> otherwise.</returns>
        bool CanValidate(int counterId);

        /// <summary>
        /// Validates the digits and precision of the raw counter value.
        /// </summary>
        /// <param name="digits">The number of digits that is detected.</param>
        /// <param name="precision">The precision that is detected.</param>
        /// <returns><see langword="true" /> if the digits and precision numbers are correct for this counter, or <see langword="false" /> otherwise.</returns>
        bool Validate(int digits, int precision);

        /// <summary>
        /// Formats an error message for the value/counter.
        /// </summary>
        /// <param name="parsedValue">The raw parsed message.</param>
        /// <param name="counterId">The counter id.</param>
        /// <returns>The error message.</returns>
        string FormatError(string parsedValue, int counterId);
    }
}