using System;
using System.IO;
using System.Text.RegularExpressions;
using QboxNext.Core.CommandLine;
using QboxNext.MergeQbx.Utils;
using QboxNext.Qserver.Core.DataStore;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.MergeQbx
{
    class Program
    {
        [Option("", "original", Required = true, HelpText = "Path to original QBX file")]
        public string OriginalQbxPath { get; set; }

        [Option("", "new", Required = true, HelpText = "Path to new QBX file")]
        public string NewQbxPath { get; set; }

        static void Main(string[] args)
        {
            var program = new Program();
            var settings = new CommandLineParserSettings { IgnoreUnknownArguments = true, CaseSensitive = false };
            ICommandLineParser parser = new CommandLineParser(settings);
            if (parser.ParseArguments(args, program, System.Console.Error))
            {
                program.Run();
            }
            else
            {
                Console.WriteLine("Usage: QboxNext.MergeQbx --original=<path to original QBX file> --new=<path to new QBX file>");
            }
        }

        private void Run()
        {
            if (!File.Exists(OriginalQbxPath))
            {
                Console.Error.WriteLine($"Could not find file ${OriginalQbxPath}");
                Environment.Exit(-1);
            }
            if (!File.Exists(NewQbxPath))
            {
                Console.Error.WriteLine($"Could not find file ${NewQbxPath}");
                Environment.Exit(-1);
            }

            using (var originalStorageProvider = GetStorageProviderForPath(OriginalQbxPath))
            {
                using (var newStorageProvider = GetStorageProviderForPath(NewQbxPath))
                {
                    originalStorageProvider.Merge(newStorageProvider);
                }
            }
        }

        private kWhStorage GetStorageProviderForPath(string originalQbxPath)
        {
            var serial = QbxPathUtils.GetSerialFromPath(originalQbxPath);
            var baseDir = QbxPathUtils.GetBaseDirFromPath(originalQbxPath);
            var counterId = QbxPathUtils.GetCounterIdFromPath(originalQbxPath);
            var storageId = QbxPathUtils.GetStorageIdFromPath(originalQbxPath);
            return new kWhStorage(serial, baseDir, counterId, Precision.mWh, storageId);
        }

    }
}
