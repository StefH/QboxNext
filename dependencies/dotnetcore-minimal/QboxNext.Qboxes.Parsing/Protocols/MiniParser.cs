using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QboxNext.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// Abstract base class for the Qbox Mini message parsers 
    /// Different firmware versions of the Qbox Mini have different implementations of the 
    /// message structure. The structure is not generic and must be specifically implemented
    /// for every changed version.
    /// </summary>
    public abstract class MiniParser
    {
        private static readonly ILogger Log = QboxNextLogProvider.CreateLogger<MiniParser>();

        protected StringParser Parser;
        protected BaseParseResult BaseParseResult;

        /// <summary>
        /// Main method for the parsing of the message.
        /// This method should not throw any exception because it would disrupt the flow
        /// of the qbox data dump. Qboxes hold their data if presented with any error response
        /// So the qbox would fill up with unhandled messages.
        /// </summary>
        /// <param name="source">the text to be parsed</param>
        /// <returns>a base result object that holds the resulting model objects</returns>
        public BaseParseResult Parse(string source)
        {
            Log.LogTrace("Enter");
            BaseParseResult = new MiniParseResult();
            try
            {
                Parser = new StringParser(source);
                DoParse();       
            }
            catch (Exception e)
            {
                BaseParseResult = new ErrorParseResult(BaseParseResult)
                {
                    Error = String.Format("Source: {0}{1}Error: {2} {3}", source, Environment.NewLine, e.Message, e.InnerException == null ? "" : e.InnerException.Message)
                };
                Log.LogError(e, "{0} (source = {1})", e.Message, source);
            }
     
            Log.LogTrace("Return");
            return BaseParseResult;
        }


		/// <summary>
		/// Can the message be parsed?
		/// </summary>
		/// <remarks>
		/// This is checked by actually parsing the message, so only use in contexts that are not performance critical.
		/// </remarks>
		public bool CanParse(string source)
		{
            BaseParseResult = new MiniParseResult();
            try
            {
                Parser = new StringParser(source);
                DoParse();
				return true;
			}
            catch
            {
	            return false;
            }
		}


        protected abstract void DoParse();

        /// <summary>
        /// Add counterpayload to payloads
        /// InvalidFormatException will be catched and logged on warn level
        /// </summary>
        /// <param name="payloads"></param>
        /// <param name="nbr"></param>
        /// <param name="value"></param>
        /// <param name="balancedParentheses">check on number of '(' is equal to number of ')'</param>
        protected void AddCounterPayload(IList<BasePayload> payloads, int nbr, string value, bool balancedParentheses)
        {
            try
            {
                if (balancedParentheses && value != null && (value.Count('('.Equals) != value.Count(')'.Equals)))
                {
                    Log.LogWarning("Parentheses not in balance for counter {nbr}: {value}", nbr, value);
                }
                else
                {
                    var payload = new CounterPayload
                    {
                        InternalNr = nbr,
                        Value = Parser.ReadSmartMeterCounterValue(value, nbr)
                    };

                    if (payload.Value < ulong.MaxValue)
                        payloads.Add(payload);
                }
            }
            catch (InvalidFormatException)
            {
                Log.LogWarning("Invalid syntax of value for counter {nbr}: {value} not in format XXXXX.XXX", nbr, value);
            }
        }


	    /// <summary>
	    /// Add counterpayload consisting of multiple values to payloads
	    /// InvalidFormatException will be catched and logged on warn level
	    /// </summary>
	    /// <param name="payloads"></param>
	    /// <param name="nbr"></param>
	    /// <param name="values"></param>
	    /// <param name="balancedParentheses">check on number of '(' is equal to number of ')'</param>
	    protected void AddCounterPayload(IList<BasePayload> payloads, int nbr, List<string> values, bool balancedParentheses)
        {
			if (!values.Any())
				return;

            var payload = new CounterPayload
            {
                InternalNr = nbr
            };

	        foreach (var value in values)
	        {
				if (balancedParentheses && value != null && (value.Count('('.Equals) != value.Count(')'.Equals)))
				{
					Log.LogWarning("Parentheses not in balance for counter {nrb}: {value}", nbr, value);
					return;
				}

				try
				{
					payload.Value += Parser.ReadSmartMeterCounterValue(value, nbr);
				}
				catch (InvalidFormatException)
				{
					Log.LogWarning("Invalid syntax of value for counter {nrb}: {value} not in format XXXXX.XXX", nbr, value);
					return;
				}
	        }

            if (payload.Value < ulong.MaxValue)
                payloads.Add(payload);
        }
    }
}