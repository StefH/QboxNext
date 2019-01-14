using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine.Text;
using QboxNext.Core.CommandLine;

namespace SimulateQbox
{
    public class QboxSimulatorOptions : CommandLineOptionsBase
    {
        public QboxSimulatorOptions()
        {                        
            OnlySendOnce = false;            
            ResetSequenceNr = false;
        }

        protected string GetParseErrors()
        {
            // todo: inherited CommandLineOptionsBase gebruikt ...
            // errors ook bij andere classes tonen...
            var help = new HelpText();
            if (this.LastPostParsingState.Errors.Count > 0)
            {
                var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }

            return help;
        }
                
        [Option("1", "once", Required = false, HelpText = "Exit after sending one message")]
        public bool OnlySendOnce { get; set; }

        [Option("p", "pattern", Required = false, HelpText = "Pattern, see below")]
        public string Pattern { get; set; }
        
        [Option("b", "qserver", Required = true, HelpText = "Qserver base URL, for example https://qserver-tst.QboxNext.nl")]
        public string QserverBaseUrl { get; set; }

        [Option("s", "qboxserial", Required = true, HelpText = "Qbox serial number: may contain format specifier, for example 99-99-{0:000}-{1:000}")]
        public string QboxSerialNr { get; set; }

        [Option("d", "nodelay", Required = false, DefaultValue = false, HelpText = "Do not wait between messages")]
	    public bool NoDelay { get; set; }

		[Option("m", "metertype", Required = false, DefaultValue = "generic", HelpText = "Meter type: <ferraris|ferrariss0|led|smart|smarts0|soladin|generic>")]
        public string MeterType { get; set; }

		[Option("u", "isduo", Required = false, DefaultValue = false, HelpText = "Simulate Qbox Duo")]
		public bool IsDuo { get; set; }

		[Option("c", "noclientstatus", Required = false, DefaultValue = false, HelpText = "Don't send client status")]
		public bool DontSendClientStatus { get; set; }

		public bool SendClientStatus { get { return !DontSendClientStatus; } }

        [Option("r", "reset", Required = false, HelpText = "Reset sequence number")]
        public bool ResetSequenceNr { get; set; }

        [Option("i", "instances", Required = false, DefaultValue = 1, HelpText = "Nr of instances")]
        public int Instances { get; set; }

		[Option("v", "version", Required = false, DefaultValue = 0x28, HelpText = "Version nr of protocol, default 0x28")]
		public int ProtocolVersion { get; set; }

        [Option("?", "help", Required = false, DefaultValue = false, HelpText = "Help")]
        public bool ShowHelp { get; set; }

		[HelpOption]
        public string GetHelp()
        {
			var usage = new StringBuilder();

			usage.AppendLine(HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current)));
			usage.AppendLine(@"A pattern consists of one or more pattern parts: <counter>:<part>[;<counter>:<part>...].

A counter can be:
	1 or 3 for a Ferraris
	1 for a LED
	181, 182, 281 or 282 for a smart meter, electricity
	2421 for a smart meter, gas

A pattern part has the form <shape>[(<scale>[,<period>[,<sequence offset>[,<counter offset>]])]
where shape is one of
	zero
	zero_peak
	flat
	flat_peak
	random
	block
	block_half
	sine.
Note that the shape is the shape of the power graph
scale is the multiplier for all counter values of that part,
period is the number of measurements in each period (not relevant for flat
shape), sequence offset is the amount of measurements to skip before starting the
series and counter offset is the base offset of the counter.

Examples:

LED-meter showing constant electricity consumption:
SimulateQbox --qserver=http://qserver-dev.QboxNext.nl --qboxserial=00-00-000-01 --pattern=1:flat --metertype=led

Smart meter showing constant electricity consumption, switching generation
and a sine wave for gas usage:
SimulateQbox --qserver=http://qserver-dev.QboxNext.nl --qboxserial=00-00-000-01 --pattern=181:flat(2);182:block(0.5,60,15);2421:sine --metertype=smart
");
			return usage.ToString();
        }
    }
}
