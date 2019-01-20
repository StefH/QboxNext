using Microsoft.Extensions.Logging;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Protocols;
using System;
using System.Collections.Generic;

namespace QboxNext.Qboxes.Parsing.Factories
{
    public static class ParserFactory
    {
        private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger("ParserFactory");
        private static readonly Dictionary<string, ParserInfo> RegisteredParsers = new Dictionary<string, ParserInfo>();

        /// <summary>
        /// Register parsers
        /// </summary>
        /// <param name="parserType"></param>
        /// <param name="ProtocolVersion">Parsers start from protocolversion</param>
        public static void Register(Type parserType, int ProtocolVersion)
        {
            if (parserType.IsSubclassOf(typeof(MiniParser)) && !RegisteredParsers.ContainsKey(parserType.Name))
            {
                RegisteredParsers.Add(parserType.Name, new ParserInfo { Type = parserType, ProtocolNr = ProtocolVersion });
            }
        }

        public static void RegisterAllParsers()
        {
            Register(typeof(MiniR07), 0x02);
            Register(typeof(MiniR16), 0x27);
            Register(typeof(MiniR21), 0x29);
            Register(typeof(MiniResponse), -1);
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
                Logger.LogError(ex, "Can't parse protocol from message {0}", message, ex);
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