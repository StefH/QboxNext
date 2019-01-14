using System;
using System.Collections.Generic;
using System.Diagnostics;
using QboxNext.Qboxes.Parsing.Logging;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Factories
{
	public static class ParserFactory
    {
		private static readonly ILog Logger = LogProvider.GetLogger("ParserFactory");
        private static readonly Dictionary<string, ParserInfo> RegisteredParsers = new Dictionary<string, ParserInfo>();

        /// <summary>
        /// Register parsers
        /// </summary>
        /// <param name="parserType"></param>
        /// <param name="ProtocolVersion">Parsers start from protocolversion</param>
        public static void Register(Type parserType, int ProtocolVersion)
        {
            Debug.Assert(RegisteredParsers != null);
            if (parserType.IsSubclassOf(typeof(MiniParser)) && !RegisteredParsers.ContainsKey(parserType.Name))
            {
                RegisteredParsers.Add(parserType.Name, new ParserInfo { Type = parserType, ProtocolNr = ProtocolVersion });
            }
        }

        public static void RegisterAllParsers()
        {
            ParserFactory.Register(typeof(MiniR07), 0x02);
            ParserFactory.Register(typeof(MiniR16), 0x27);
			ParserFactory.Register(typeof(MiniR21), 0x29);
            ParserFactory.Register(typeof(MiniResponse), -1);
        }

        public static MiniParser GetParser(string name)
        {
            return Activator.CreateInstance(RegisteredParsers[name].Type) as MiniParser;
        }

        /// <summary>
        /// First 2 bytes (= protocolversion) is depended for parser
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
		public static MiniParser GetParserFromMessage(string message)
		{
			int protocolNr;
			try
			{
	            protocolNr = int.Parse(message.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			}
			catch (FormatException ex)
			{
				Logger.ErrorException("Can't parse protocol from message " + message, ex);
				throw;
			}

            ParserInfo pi = null;
            foreach (var item in RegisteredParsers.Values)
                if ((pi == null) || ((item.ProtocolNr <= protocolNr) && (pi.ProtocolNr < item.ProtocolNr)))
                    pi = item;

            if (pi == null)
                throw new Exception("No parser found in ParserFactory");

            return GetParser(pi.Type.Name);
		}
    }
}