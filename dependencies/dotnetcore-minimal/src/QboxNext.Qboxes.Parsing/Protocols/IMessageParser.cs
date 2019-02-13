namespace QboxNext.Qboxes.Parsing.Protocols
{
    public interface IMessageParser
    {
        /// <summary>
        /// Main method for the parsing of the message.
        /// This method should not throw any exception because it would disrupt the flow
        /// of the qbox data dump. Qboxes hold their data if presented with any error response
        /// So the qbox would fill up with unhandled messages.
        /// </summary>
        /// <param name="message">the text to be parsed</param>
        /// <returns>a base result object that holds the resulting model objects</returns>
        BaseParseResult Parse(string message);

        /// <summary>
        /// Checks if the message can be parsed.
        /// </summary>
        bool CanParse(string source);
    }
}
