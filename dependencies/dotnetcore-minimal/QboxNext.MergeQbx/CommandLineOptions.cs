using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace QboxNext.MergeQbx
{
    public class CommandLineOptions
    {
        [Option("original", Required = true, HelpText = "Path to original QBX file")]
        public string OriginalQbxPath { get; set; }

        [Option("new", Required = true, HelpText = "Path to new QBX file")]
        public string NewQbxPath { get; set; }

        [Usage(ApplicationAlias = "dotnet run")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                var opts = UnParserSettings.WithUseEqualTokenOnly();

                yield return new Example("Merge QBX file", UnParserSettings.WithUseEqualTokenOnly(), new CommandLineOptions { OriginalQbxPath = "<path_to_original_QBX_file>", NewQbxPath = "<path_to_new_QBX_file>" });
            }
        }
    }
}