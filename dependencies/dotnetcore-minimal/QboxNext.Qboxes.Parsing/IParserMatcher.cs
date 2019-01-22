namespace QboxNext.Qboxes.Parsing
{
    public interface IParserMatcher
    {
        /// <summary>
        /// Gets the matching <see cref="ParserInfo" /> for a specific message.
        /// </summary>
        /// <param name="message">The message to analyze.</param>
        /// <returns>The parser info.</returns>
        ParserInfo Match(string message);
    }
}
