using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QboxNext.Simulation;

namespace QboxNext.SimulateQbox
{
    public class ConsoleRunner : IHostedService
    {
        private readonly ILogger<ConsoleRunner> _logger;
        private readonly QboxSimulatorOptions _options;
        private readonly IList<QboxSimulator> _simulators = new List<QboxSimulator>();

        public ConsoleRunner(ILogger<ConsoleRunner> logger, IOptions<QboxSimulatorOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!CheckArguments(_options))
            {
                _logger.LogError("invalid parameter usage");
                return Task.CompletedTask;
            }

            RunSimulators(_options);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (QboxSimulator simulator in _simulators)
            {
                simulator.Stop();
            }

            return Task.CompletedTask;
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
        private void RunSimulators(QboxSimulatorOptions inOptions)
        {
            for (var i = 0; i < inOptions.Instances; i++)
            {
                var serial = string.Format(inOptions.QboxSerialNr, i / 1000, i % 1000);
                var simulator = CreateSimulator(serial, inOptions);

                _simulators.Add(simulator);

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
                    foreach (var simulator in _simulators)
                    {
                        if (simulator.State != SimulatorState.Stopped)
                            isAnySimulatorRunning = true;
                    }
                    if (isAnySimulatorRunning)
                        Thread.Sleep(100);
                }
                while (isAnySimulatorRunning);
            }
        }
    }
}