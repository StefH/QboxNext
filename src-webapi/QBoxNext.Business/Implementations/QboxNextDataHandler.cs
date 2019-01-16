﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Qboxes.Classes;
using Qboxes.Interfaces;
using QboxNext.Common.Validation;
using QboxNext.Core.Dto;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Factories;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;
using QBoxNext.Business.Interfaces.Internal;

namespace QBoxNext.Business.Implementations
{
    /// <summary>
    /// Class that encapsulates the handling of the Qbox mini data dump.
    ///
    /// Based on <see cref="MiniDataHandler"/> but with fixes for:
    /// - handle Payload.Visit(...) exceptions correctly
    /// </summary>
    public class QboxNextDataHandler : IVisitorAsync
    {
        private static readonly Dictionary<int, int> SmartMeterIdMapping = new Dictionary<int, int>
        {
           { 1, 181 },
           { 2, 182 },
           { 3, 281 },
           { 4, 282 },
           { 5, 270 },
           { 6, 170 },
           { 7, 2421 }
        };
        private static readonly Dictionary<int, int> SoladinIdMapping = new Dictionary<int, int>
        {
           { 3, 120 }
        };
        private static readonly DateTime Epoch = new DateTime(2007, 1, 1);

        private readonly QboxDataDumpContext _context;
        private readonly IQboxMessagesLogger _qboxMessagesLogger;
        private readonly ILogger<QboxNextDataHandler> _logger;

        private BaseParseResult _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxNextDataHandler"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="qboxMessagesLogger">The qbox messages logger.</param>
        /// <param name="logger">The logger.</param>
        public QboxNextDataHandler([NotNull] QboxDataDumpContext context, [NotNull] IQboxMessagesLogger qboxMessagesLogger, [NotNull] ILogger<QboxNextDataHandler> logger)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(qboxMessagesLogger, nameof(qboxMessagesLogger));
            Guard.NotNull(logger, nameof(logger));

            _context = context;
            _qboxMessagesLogger = qboxMessagesLogger;
            _logger = logger;
        }

        public async Task<string> HandleAsync()
        {
            using (new ExtendedLogger(_context.Mini.SerialNumber))
            {
                _logger.LogTrace("Enter");
                try
                {
                    _logger.LogInformation("sn: {0} | input: {1} | lastUrl: {2}", _context.Mini.SerialNumber, _context.Message, _context.Mini.QboxStatus.Url);

                    LogQboxMessage(_context.Message, QboxMessageType.Request);

                    // start parsing
                    var parser = ParserFactory.GetParserFromMessage(_context.Message);
                    _result = parser.Parse(_context.Message);
                    // end of parsing

                    if (_result is MiniParseResult parseResult)
                    {
                        // handle the result
                        _context.Mini.QboxStatus.FirmwareVersion = parseResult.ProtocolNr;
                        _context.Mini.State = parseResult.Model.Status.Status;
                        _context.Mini.QboxStatus.State = (byte)parseResult.Model.Status.Status;

                        bool operational = false;

                        switch (_context.Mini.State)
                        {
                            case MiniState.HardReset:
                                _context.Mini.QboxStatus.LastHardReset = DateTime.UtcNow;
                                break;
                            case MiniState.InvalidImage:
                                _context.Mini.QboxStatus.LastImageInvalid = DateTime.UtcNow;
                                break;
                            case MiniState.Operational:
                                operational = true;
                                break;
                            case MiniState.ValidImage:
                                _context.Mini.QboxStatus.LastImageValid = DateTime.UtcNow;
                                break;
                            case MiniState.UnexpectedReset:
                                _context.Mini.QboxStatus.LastPowerLoss = DateTime.UtcNow;
                                break;
                        }

                        if (!operational)
                        {
                            _context.Mini.QboxStatus.LastNotOperational = DateTime.UtcNow;
                        }

                        if (parseResult.Model.Status.TimeIsReliable)
                        {
                            _context.Mini.QboxStatus.LastTimeIsReliable = DateTime.UtcNow;
                        }
                        else
                        {
                            _context.Mini.QboxStatus.LastTimeUnreliable = DateTime.UtcNow;
                        }

                        if (parseResult.Model.Status.ValidResponse)
                        {
                            _context.Mini.QboxStatus.LastValidResponse = DateTime.UtcNow;
                        }
                        else
                        {
                            _context.Mini.QboxStatus.LastInvalidResponse = DateTime.UtcNow;
                        }

                        // Loop all payloads except CounterPayload and visit
                        foreach (var payload in parseResult.Model.Payloads.Where(p => !(p is CounterPayload)))
                        {
                            payload.Visit(this);
                        }

                        // Loop all CounterPayloads and store each measurement
                        foreach (var counterPayload in parseResult.Model.Payloads.Where(p => p is CounterPayload).Cast<CounterPayload>())
                        {
                            await AcceptAsync(counterPayload);
                        }

                        BuildResult(ResponseType.Normal);
                    }
                    else
                    {
                        if (_result is ErrorParseResult errorParseResult)
                        {
                            LogQboxMessage(errorParseResult.Error, QboxMessageType.Error);
                        }

                        // We could not handle the message normally, but if we don't answer at all, the Qbox will just retransmit the message.
                        // So we just send back the basic message, without handling the queue and auto-answer.
                        BuildResult(ResponseType.Basic);
                    }

                    _context.Mini.QboxStatus.LastSeen = DateTime.UtcNow;

                    _logger.LogDebug("sn: {0} | result: {1}", _context.Mini.SerialNumber, _result.GetMessage());

                    LogQboxMessage(_result.GetMessageWithEnvelope(), QboxMessageType.Response);

                    return _result.GetMessageWithEnvelope();
                }
                catch (Exception exception)
                {
                    if (_context.Mini != null)
                    {
                        _context.Mini.QboxStatus.LastSeen = DateTime.UtcNow;
                        _context.Mini.QboxStatus.LastError = DateTime.UtcNow;
                        _context.Mini.QboxStatus.LastErrorMessage = exception.Message;
                    }

                    LogQboxMessage(exception.ToString(), QboxMessageType.Exception);
                    _logger.LogError(exception, $"sn: {_context.Mini.SerialNumber}");

                    throw;
                }
            }
        }

        private void BuildResult(ResponseType inResponseType)
        {
            // sequence number of the message received
            _result.Write((byte)_result.SequenceNr);

            // time in seconds from 1-1-2007
            var seconds = Convert.ToInt32(DateTime.Now.Subtract(Epoch).TotalSeconds);
            _result.Write(seconds);

            _result.Write(_context.Mini.Offset);

            // SequenceNr is 0-255.
            bool canSendAutoStatusCommand = inResponseType == ResponseType.Normal && CanSendAutoStatusCommand((byte)_result.SequenceNr);
            bool canSendAutoAnswerCommand = inResponseType == ResponseType.Normal;
            var responseBuilder = new QboxResponseBuilder(_context.Mini, ClientRepositories.Queue, canSendAutoStatusCommand, canSendAutoAnswerCommand);

            var commands = responseBuilder.Build();
            if (commands != null && commands.Count > 0)
            {
                _result.Write(commands[0]);
                QueueCommands(commands.Skip(1));
            }
        }

        /// <summary>
        /// Put commands in the Qbox' command queue.
        /// </summary>
        private void QueueCommands(IEnumerable<string> inCommands)
        {
            // When resubmitting, there is no queue.
            if (ClientRepositories.Queue == null)
            {
                return;
            }

            foreach (var command in inCommands)
            {
                ClientRepositories.Queue.Enqueue(_context.Mini.SerialNumber, command);
            }
        }

        /// <summary>
        /// result = true when Mini has 1 client and clientstatus 'ConnectionWithClient' = false
        /// Check with max. 1 client, due to the missing client info in payload (duo)
        /// </summary>
        /// <returns></returns>
        private bool ClientNotConnected()
        {
            if (_context.Mini.QboxStatus.ClientStatuses.Count == 1)
            {
                var state = new ClientMiniStatus(_context.Mini.QboxStatus.ClientStatuses.First().Value, _context.Mini.QboxStatus.FirmwareVersion);
                return !state.ConnectionWithClient;
            }

            return false;
        }

        private CounterPoco FindCounter(CounterPayload payload)
        {
            if (payload is CounterWithSourcePayload withSourcePayload)
            {
                var counter = _context.Mini.Counters
                    .SingleOrDefault(s => s.CounterId == payload.InternalNr &&
                                          s.Secondary != withSourcePayload.PrimaryMeter &&
                                          s.Groupid == withSourcePayload.Source);

                if (counter != null)
                {
                    return counter;
                }

                // No specific counter found, try to find counter with only internalNr
            }

            // Down grading firmware could lead to find multiple counters. InternalId is not unique from firmware revision 39
            return _context.Mini.Counters.SingleOrDefault(s => s.CounterId == payload.InternalNr);
        }

        #region Implementation of IVisitorAsync

        public void Accept(CounterPayload payload)
        {
            throw new NotSupportedException("Use AcceptAsync(...)");
        }

        /// <summary>
        /// Process the payload
        /// </summary>
        /// <param name="payload">Payload, InternalNr will be mapped to the actual internal number used to store the data.</param>
        public async Task AcceptAsync(CounterPayload payload)
        {
            if (payload.Value == ulong.MaxValue)
            {
                return;
            }

            if (payload is R21CounterPayload counterPayload && !counterPayload.IsValid)
            {
                _logger.LogDebug("Invalid value for counter {0} / {1}", counterPayload.InternalNr, _context.Mini.SerialNumber);
                return;
            }

            if (ClientNotConnected())
            {
                // No connection with client, payload is last measured value and not the real value. First real value will be spread out over missing values
                _logger.LogDebug("No connection with client, data not saved");
                return;
            }

            if (!MapCounterId(payload, _context.Mini))
            {
                return;
            }

            // store the data in the payload into the corresponding counter
            var counter = FindCounter(payload);
            if (counter == null)
            {
                _logger.LogWarning($"Received value for unknown counter: {payload.InternalNr} / {_context.Mini.SerialNumber}");
                return; //todo: investigate if exception would be better 
            }

            if (!(_result is MiniParseResult parseResult))
            {
                return;
            }

            if (counter.StorageProvider is IStorageProviderAsync asyncStorageProvider)
            {
                await asyncStorageProvider.StoreValueAsync(parseResult.Model.MeasurementTime, payload.Value, counter.CounterSensorMappings.First().Formule);
            }
            else
            {
                throw new NotSupportedException("Only providers which implement IStorageProviderAsync are supported.");
            }

            // counter.SetValue(parseResult.Model.MeasurementTime, payload.Value, _context.Mini.QboxStatus);

            _context.Mini.QboxStatus.LastDataReceived = DateTime.UtcNow;
        }

        public void Accept(DeviceSettingsPayload payload)
        {
            try
            {
                if (Enum.GetValues(typeof(DeviceSettingType)).Cast<DeviceSettingType>().Contains(payload.DeviceSetting))
                {
                    string key = payload.DeviceSetting.ToString();
                    _context.Mini.QboxStatus.DebugSettings[key] = payload.DeviceSettingValueStr;
                    _context.Mini.QboxStatus.DebugSettingsLastReceived[key] = DateTime.UtcNow;
                }
            }
            catch (Exception e)
            {
                //todo: add specific handling for file locking etc iso this Pokemon... (rolf)
                _logger.LogError(e, e.Message);
            }
        }

        public void Accept(ClientStatusPayload payload)
        {
            try
            {
                // qplat-74 implementation
                var client = (QboxClient)payload.Client;
                _context.Mini.QboxStatus.ClientStatuses[client.ToString()] = payload.RawValue;

                _context.Mini.QboxStatus.ClientStateDates[$"{client.ToString()}-{payload.State.State.ToString()}"] = payload.MeasurementTime;
            }
            catch (Exception e)
            {
                //todo: add specific handling for file locking etc iso this Pokemon... (rolf)
                _logger.LogError(e, e.Message);
            }
        }
        #endregion

        /// <summary>
        /// Change the InternalId (CounterId) of the payload to the value that will be used to store and retrieve the measurements.
        /// </summary>
        /// <returns>false if the mapping could not be done (for example when the secondary meter type of a duo is not an S0 meter).</returns>
        public bool MapCounterId(CounterPayload ioPayload, MiniPoco inMini)
        {
            Dictionary<int, int> mapping = null;

            var counterWithSourcePayload = ioPayload as CounterWithSourcePayload;

            if (counterWithSourcePayload == null || counterWithSourcePayload.PrimaryMeter)
            {
                // Firmware A34, Smart Meter and soladin measured on client reports counters instead of specific message:
                if (inMini.Clients.Any(d =>
                    d.MeterType == DeviceMeterType.Smart_Meter_E || d.MeterType == DeviceMeterType.Smart_Meter_EG))
                {
                    mapping = SmartMeterIdMapping;
                }
                else if (inMini.Clients.Any(d => d.MeterType == DeviceMeterType.Soladin_600))
                {
                    mapping = SoladinIdMapping;
                }
            }

            if (counterWithSourcePayload != null && !counterWithSourcePayload.PrimaryMeter)
            {
                // At the moment we only support S0 as secondary meter type, so log an error when we find a different meter type.
                var clientsWithUnsupportedSecondaryMeterTypes = inMini.Clients.Where(c => c.SecondaryMeterType != DeviceMeterType.None &&
                                                                                          c.SecondaryMeterType != DeviceMeterType.SO_Pulse).ToList();
                if (clientsWithUnsupportedSecondaryMeterTypes.Count > 0)
                {
                    _logger.LogError("Qbox {0} has counter for unsupported secondary meter type {1}", inMini.SerialNumber,
                            clientsWithUnsupportedSecondaryMeterTypes[0].SecondaryMeterType);
                    return false;
                }

                // S0 as secondary meter type does not need to be mapped.
            }

            if (mapping != null && mapping.ContainsKey(ioPayload.InternalNr))
            {
                ioPayload.InternalNr = mapping[ioPayload.InternalNr];
            }

            return true;
        }

        /// <summary>
        /// Can we send the auto status commands on the specified sequence nr?
        /// TODO : return always false for now
        /// </summary>
        private bool CanSendAutoStatusCommand(byte inSequenceNr)
        {
            return false;
        }

        private void LogQboxMessage(string message, QboxMessageType messageType)
        {
            _qboxMessagesLogger.LogQboxMessage(_context.Mini.SerialNumber, message, messageType);
        }

        private enum ResponseType
        {
            Basic,
            Normal
        }
    }
}