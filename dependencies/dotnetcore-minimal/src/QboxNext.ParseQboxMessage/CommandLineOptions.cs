using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace QboxNext.ParseQboxMessage
{
    public class CommandLineOptions
    {
        [Option('m', "message", Required = true, HelpText = "Qbox message")]
        public string Message { get; set; }

        [Usage(ApplicationAlias = "dotnet run")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                var opts = UnParserSettings.WithUseEqualTokenOnly();

                yield return new Example("Parse Qbox message", opts, new CommandLineOptions { Message = "<message>" });
            }
        }
    }
}