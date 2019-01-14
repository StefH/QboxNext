using System;

namespace QboxNext.Qboxes.Parsing.Exceptions
{
    internal class ElementValueHandlerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ElementValueHandlerException class.
        /// Used as a wrapper for exception thrown inside a method of the 
        /// ElementValueHandler class.
        /// </summary>
        /// <param name="message">Error message explaining the exception</param>
        /// <param name="E">Inner exception</param>
        public ElementValueHandlerException(string message, Exception E)
        {  }
    }
}
