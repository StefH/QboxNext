// SimulateQbox simulates a Qbox attached to any meter.
// It will send immediately, and then after each minute, so it will not send on the minute boundary.
// It saves the sequence nr of the Qbox so it can be safely stopped and started.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using NLog;
using NLog.Extensions.Logging;
using QboxNext.Core.CommandLine;
using QboxNext.Core.Simulation;
using QboxNext.Logging;
using QboxNext.SimulateQbox;

namespace SimulateQbox
{
	class Program 
	{
	    private static readonly Logger Log = LogManager.GetLogger("Program");
		
        static void Main(string[] args)
		{
            // Setup static logger factory
		    QboxNextLogProvider.LoggerFactory = new NLogLoggerFactory();

            Log.Info("Starting...");

			// Make sure that the default parsing and formatting of numbers is using '.' as floating point.
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			Log.Debug(string.Format("SimulateQbox {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
			
            var settings = new CommandLineParserSettings { IgnoreUnknownArguments = true, CaseSensitive = false };
            ICommandLineParser parser = new CommandLineParser(settings);
            
            var options = new QboxSimulatorOptions();
            if (parser.ParseArguments(args, options, Console.Error) && CheckArguments(options))
            {
				RunSimulators(options);
                Log.Info("Finishing...");
            }
            else
			{
                Log.Error("invalid parameter usage");
			}

            Log.Info("Closing");
		    
		    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
		    NLog.LogManager.Shutdown();
		}


		/// <summary>
		/// Create simulator from command line options.
		/// </summary>
		private static QboxSimulator CreateSimulator(string inSerial, QboxSimulatorOptions inOptions)
		{
			var sim = new QboxSimulator(inSerial)
			{
				Host = inOptions.QserverBaseUrl,
				QboxDeviceName = "666",
				QboxIsDuo = inOptions.IsDuo,
				SendClientStatus = inOptions.SendClientStatus,
				ProtocolVersion = inOptions.ProtocolVersion,
				LoadPersistedSequenceNr = !inOptions.ResetSequenceNr,
				RunContinuously = !inOptions.OnlySendOnce,
				// Add 2 seconds to give the simulator time to start up, it will try to start at exactly the given timestamp.
				StartTime = DateTime.Now.AddSeconds(2)
			};

			if (inOptions.NoDelay)
				sim.MessageInterval = new TimeSpan(0, 0, 1);

			if (inOptions.MeterType == MeterTypeName.Generic && !sim.RunContinuously)
				throw new InvalidConstraintException("generic meter can only be used in continuous mode");

			if (inOptions.MeterType == MeterTypeName.Generic)
				sim.QboxState = QboxState.Waiting;

			var specs = PatternParser.ParseSpecList(inOptions.Pattern);
			// To make sure that simulators that run once every minute can progress in the pattern,
			// we have to fix the period start.
			if (inOptions.OnlySendOnce)
				specs.ForEach(s => s.PeriodStart = new DateTime(2017, 1, 1));
			sim.SetMeter(Meter.GetMeterType(inOptions.MeterType), specs);

			return sim;
		}


		/// <summary>
		/// Check the consistency of the supplied command line arguments.
		/// </summary>
		private static bool CheckArguments(QboxSimulatorOptions inOptions)
		{
			if (inOptions.MeterType == MeterTypeName.Generic && inOptions.OnlySendOnce)
				return false;
			else
				return true;
		}


		/// <summary>
		/// Run all simulators specified by the options.
		/// </summary>
		private static void RunSimulators(QboxSimulatorOptions inOptions)
		{
			var simulators = new List<QboxSimulator>();

			for (var i = 0; i < inOptions.Instances; i++)
			{
				var serial = string.Format(inOptions.QboxSerialNr, i / 1000, i % 1000);
				var simulator = CreateSimulator(serial, inOptions);

				simulators.Add(simulator);

				new Thread(() => simulator.Start()).Start();

				while (simulator.State != SimulatorState.Playing)
				{
					Thread.Sleep(1);
				}
			}

			if (inOptions.OnlySendOnce)
			{
				bool isAnySimulatorRunning;
				do
				{
					isAnySimulatorRunning = false;
					foreach (var simulator in simulators)
					{
						if (simulator.State != SimulatorState.Stopped)
							isAnySimulatorRunning = true;
					}
					if (isAnySimulatorRunning)
						Thread.Sleep(100);
				}
				while (isAnySimulatorRunning);
			}
			else
			{
				Console.WriteLine("Press enter to stop!");
				Console.ReadLine();

				foreach (var simulator in simulators)
					simulator.Stop();
			}
		}
	}
}
