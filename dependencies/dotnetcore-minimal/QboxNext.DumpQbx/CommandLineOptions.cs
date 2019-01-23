using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace QboxNext.DumpQbx
{
    public class CommandLineOptions
    {
        [Option("qbx", Required = true, HelpText = "Path to QBX file")]
        public string QbxPath { get; set; }

        [Option("values", Required = false, HelpText = "Dump values")]
        public bool IsDumpingValues { get; set; }

        [Usage(ApplicationAlias = "dotnet run")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                var opts = UnParserSettings.WithUseEqualTokenOnly();
                yield return new Example("View qbx info", opts, new CommandLineOptions { QbxPath = "file.qbx" });
                yield return new Example("Dump qbx file", opts, new CommandLineOptions { QbxPath = "file.qbx", IsDumpingValues = true });
            }
        }
    }
}