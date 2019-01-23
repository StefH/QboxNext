using System.Text;
using CommandLine;
using QboxNext.ConsoleApp;

namespace QboxNext.SimulateQbox
{
    public class QboxSimulatorOptions : ICustomHelpText
    {
        [Option('1', "once", Required = false, HelpText = "Exit after sending one message")]
        public bool OnlySendOnce { get; set; }

        [Option('p', "pattern", Required = false, HelpText = "Pattern, see below")]
        public string Pattern { get; set; }

        [Option('b', "qserver", Required = true, HelpText = "Qserver base URL, for example https://localhost:5000")]
        public string QserverBaseUrl { get; set; }

        [Option('s', "qboxserial", Required = true, HelpText = "Qbox serial number: may contain format specifier, for example 99-99-{0:000}-{1:000}")]
        public string QboxSerialNr { get; set; }

        [Option('d', "nodelay", Required = false, Default = false, HelpText = "Do not wait between messages")]
        public bool NoDelay { get; set; }

        [Option('m', "metertype", Required = false, Default = "generic", HelpText = "Meter type: <ferraris|ferrariss0|led|smart|smarts0|soladin|generic>")]
        public string MeterType { get; set; }

        [Option('u', "isduo", Required = false, Default = false, HelpText = "Simulate Qbox Duo")]
        public bool IsDuo { get; set; }

        [Option('c', "noclientstatus", Required = false, Default = false, HelpText = "Don't send client status")]
        public bool DontSendClientStatus { get; set; }

        public bool SendClientStatus { get { return !DontSendClientStatus; } }

        [Option('r', "reset", Required = false, HelpText = "Reset sequence number")]
        public bool ResetSequenceNr { get; set; }

        [Option('i', "instances", Required = false, Default = 1, HelpText = "Nr of instances")]
        public int Instances { get; set; }

        [Option('v', "version", Required = false, Default = 0x28, HelpText = "Version nr of protocol, default 0x28")]
        public int ProtocolVersion { get; set; }

        public string GetHelpText(string defaultText)
        {
            var usage = new StringBuilder();

            usage.AppendLine(defaultText);
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
SimulateQbox --qserver=https://localhost:5000 --qboxserial=00-00-000-01 --pattern=1:flat --metertype=led

Smart meter showing constant electricity consumption, switching generation
and a sine wave for gas usage:
SimulateQbox --qserver=https://localhost:5000 --qboxserial=00-00-000-01 --pattern=181:flat(2);182:block(0.5,60,15);2421:sine --metertype=smart
");
            return usage.ToString();
        }
    }
}
