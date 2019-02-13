using System;
using System.Collections.Generic;
using System.Text;

namespace QboxNext.Model.Utils
{
    /// <summary>
    /// Class for all exceptions related to the QboxBuilder class.
    /// </summary>
    [Serializable]
    public class QboxBuilderException : System.Exception
    {
        /// <summary>
        /// Constructor for the exception class
        /// </summary>
        public QboxBuilderException() { }
        public QboxBuilderException(string message) : base(message) { }
    }
}
