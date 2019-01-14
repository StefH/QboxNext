using System;
using System.Globalization;
using System.IO;
using QboxNext.Core.CommandLine;
using QboxNext.Qserver.Core.Utils;

namespace QboxNext.DumpQbx
{
    class Program
    {
        [Option("", "qbx", Required = true, HelpText = "Path to QBX file")]
        public string QbxPath { get; set; }


        [Option("", "values", Required = false, DefaultValue = false, HelpText = "Dump values")]
        public bool IsDumpingValues { get; set; }

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
                System.Console.WriteLine("Usage: QboxNext.DumpQbx --qbx=<path> [--values]");
            }
        }

        private void Run()
        {
            using (var reader = new BinaryReader(File.Open(QbxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var startOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                var endOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                var id = new Guid(reader.ReadBytes(16));

                System.Console.WriteLine("StartOfFile: {0}", startOfFile);
                System.Console.WriteLine("EndOfFile:   {0}", endOfFile);
                System.Console.WriteLine("ID:          {0}", id);

                if (IsDumpingValues)
                {
                    System.Console.WriteLine("Timestamp NL     : {0,10}, {1,10}, {2,10}, {3,5} (kWh can be kWh, Wh or mWh depending on Precision setting)",
                        "raw", "kWh", "money", "quality");
                    var length = reader.BaseStream.Length;
                    var currentTimestamp = startOfFile;
                    while (reader.BaseStream.Position < length)
                    {
                        var raw = reader.ReadUInt64();
                        var kWh = reader.ReadUInt64();
                        var money = reader.ReadUInt64();
                        var quality = reader.ReadUInt16();

                        System.Console.Write(currentTimestamp.ToString("yyyy-MM-dd HH:mm : "));
                        if (raw == ulong.MaxValue && kWh == 0 && money == 0 && quality == 0)
                        {
                            System.Console.WriteLine("empty slot");
                        }
                        else
                        {
                            var rawDisplay = raw == ulong.MaxValue ? "<none>" : raw.ToString(CultureInfo.InvariantCulture);
                            System.Console.WriteLine("{0,10}, {1,10}, {2,10}, {3,5}", rawDisplay, kWh, money, quality);
                        }

                        currentTimestamp = currentTimestamp.AddMinutes(1);
                    }
                }
            }
        }
    }
}
