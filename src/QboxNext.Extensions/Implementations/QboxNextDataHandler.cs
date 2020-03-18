using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Interfaces;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Model.Classes;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.Extensions.Implementations
{
    /// <summary>
    /// Class that encapsulates the handling of the Qbox mini data dump.
    ///
    /// Based on <see cref="MiniDataHandler"/> but with fixes for:
    /// - handle Payload.Visit(...) exceptions correctly
    /// - Async support
    /// </summary>
    [UsedImplicitly]
    public class QboxNextDataHandler : IQboxNextDataHandler
    {
        private const string MessageTypeRequest = "Request";
        private const string MessageTypeRequestParsed = "RequestParsed";
        private const string MessageTypeResponse = "Response";
        private const string MessageTypeError = "Error";
        private const string MessageTypeException = "Exception";

        private static readonly Dictionary<int, int> SmartMeterIdMapping = new Dictionary<int, int>
        {
           { 1, QboxConstants.CounterIdConsumptionLow },
           { 2, QboxConstants.CounterIdConsumptionHigh },
           { 3, QboxConstants.CounterIdGenerationLow },
           { 4, QboxConstants.CounterIdGenerationHigh },
           { 5, 270 },
           { 6, 170 },
           { 7, QboxConstants.CounterIdGasConsumption }
        };
        private static readonly Dictionary<int, int> SoladinIdMapping = new Dictionary<int, int>
        {
           { 3, 120 }
        };
        private static readonly DateTime Epoch = new DateTime(2007, 1, 1);

        private readonly string _correlationId;
        private readonly QboxDataDumpContext _context;
        private readonly IParserFactory _parserFactory;
        private readonly ICounterStoreService _counterService;
        private readonly IStateStoreService _stateStoreService;
        private readonly ILogger<QboxNextDataHandler> _logger;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxNextDataHandler"/> class.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="context">The context.</param>
        /// <param name="parserFactory">The parser factory.</param>
        /// <param name="counterService">The counter service.</param>
        /// <param name="stateStoreService">The state store service.</param>
        /// <param name="dateTimeService">The DateTimeService</param>
        /// <param name="logger">The logger.</param>
        public QboxNextDataHandler(
            [NotNull] string correlationId,
            [NotNull] QboxDataDumpContext context,
            [NotNull] IParserFactory parserFactory,
            [NotNull] ICounterStoreService counterService,
            [NotNull] IStateStoreService stateStoreService,
            [NotNull] IDateTimeService dateTimeService,
            [NotNull] ILogger<QboxNextDataHandler> logger)
        {
            Guard.IsNotNullOrEmpty(correlationId, nameof(correlationId));
            Guard.IsNotNull(context, nameof(context));
            Guard.IsNotNull(parserFactory, nameof(parserFactory));
            Guard.IsNotNull(counterService, nameof(counterService));
            Guard.IsNotNull(stateStoreService, nameof(stateStoreService));
            Guard.IsNotNull(logger, nameof(logger));
            Guard.IsNotNull(dateTimeService, nameof(dateTimeService));

            _correlationId = correlationId;
            _context = context;
            _parserFactory = parserFactory;
            _counterService = counterService;
            _stateStoreService = stateStoreService;
            _logger = logger;
            _dateTimeService = dateTimeService;
        }

        /// <inheritdoc cref="IQboxNextDataHandler.HandleAsync()"/>
        public async Task<string> HandleAsync()
        {
            try
            {
                var stateDataRequest = new StateData
                {
                    SerialNumber = _context.Mini.SerialNumber,
                    MessageType = MessageTypeRequest,
                    Message = _context.Message,
                    State = _context.Mini.State,
                    Status = _context.Mini.QboxStatus
                };
                await _stateStoreService.StoreAsync(_correlationId, stateDataRequest);

                var parser = _parserFactory.GetParser(_context.Message);
                var result = parser.Parse(_context.Message);

                var responseMessageTime = _dateTimeService.Now;
                if (result is MiniParseResult parseResult)
                {
                    // handle the result
                    _context.Mini.QboxStatus.FirmwareVersion = parseResult.ProtocolNr;
                    _context.Mini.State = parseResult.Model.Status.Status;
                    _context.Mini.QboxStatus.State = (byte)parseResult.Model.Status.Status;

                    var stateDataRequestParsed = new StateData
                    {
                        SerialNumber = _context.Mini.SerialNumber,
                        MessageType = MessageTypeRequestParsed,
                        Message = null, // Set to null
                        State = _context.Mini.State,
                        Status = _context.Mini.QboxStatus,
                        MeterType = parseResult.Model.MeterType,
                        MessageTime = parseResult.Model.MeasurementTime,
                        SequenceNumber = parseResult.SequenceNr,
                        Payloads = parseResult.Model.Payloads?.Count
                    };
                    await _stateStoreService.StoreAsync(_correlationId, stateDataRequestParsed);

                    bool operational = false;

                    switch (_context.Mini.State)
                    {
                        case MiniState.HardReset:
                            _context.Mini.QboxStatus.LastHardReset = _dateTimeService.UtcNow;
                            break;
                        case MiniState.InvalidImage:
                            _context.Mini.QboxStatus.LastImageInvalid = _dateTimeService.UtcNow;
                            break;
                        case MiniState.Operational:
                            operational = true;
                            break;
                        case MiniState.ValidImage:
                            _context.Mini.QboxStatus.LastImageValid = _dateTimeService.UtcNow;
                            break;
                        case MiniState.UnexpectedReset:
                            _context.Mini.QboxStatus.LastPowerLoss = _dateTimeService.UtcNow;
                            break;
                    }

                    if (!operational)
                    {
                        _context.Mini.QboxStatus.LastNotOperational = _dateTimeService.UtcNow;
                    }

                    if (parseResult.Model.Status.TimeIsReliable)
                    {
                        _context.Mini.QboxStatus.LastTimeIsReliable = _dateTimeService.UtcNow;
                    }
                    else
                    {
                        _context.Mini.QboxStatus.LastTimeUnreliable = _dateTimeService.UtcNow;
                    }

                    if (parseResult.Model.Status.ValidResponse)
                    {
                        _context.Mini.QboxStatus.LastValidResponse = _dateTimeService.UtcNow;
                    }
                    else
                    {
                        _context.Mini.QboxStatus.LastInvalidResponse = _dateTimeService.UtcNow;
                    }

                    await VisitPayloadsAsync(parseResult);

                    BuildResult(parseResult, responseMessageTime, ResponseType.Normal);
                }
                else
                {
                    // We could not handle the message normally, but if we don't answer at all, the Qbox will just retransmit the message.
                    // So we just send back the basic message, without handling the queue and auto-answer.
                    BuildResult(result, responseMessageTime, ResponseType.Basic);

                    if (result is ErrorParseResult errorParseResult)
                    {
                        var stateDataError = new StateData
                        {
                            SerialNumber = _context.Mini.SerialNumber,
                            MessageType = MessageTypeError,
                            Message = errorParseResult.Error,
                            State = _context.Mini.State,
                            Status = _context.Mini.QboxStatus,
                            MessageTime = responseMessageTime
                        };
                        await _stateStoreService.StoreAsync(_correlationId, stateDataError);
                    }
                }

                _context.Mini.QboxStatus.LastSeen = _dateTimeService.UtcNow;

                string resultWithEnvelope = result.GetMessageWithEnvelope();

                var stateDataResponse = new StateData
                {
                    SerialNumber = _context.Mini.SerialNumber,
                    MessageType = MessageTypeResponse,
                    Message = resultWithEnvelope,
                    State = _context.Mini.State,
                    Status = _context.Mini.QboxStatus,
                    MessageTime = responseMessageTime
                };
                await _stateStoreService.StoreAsync(_correlationId, stateDataResponse);

                return resultWithEnvelope;
            }
            catch (Exception exception)
            {
                if (_context.Mini != null)
                {
                    _context.Mini.QboxStatus.LastSeen = _dateTimeService.UtcNow;
                    _context.Mini.QboxStatus.LastError = _dateTimeService.UtcNow;
                    _context.Mini.QboxStatus.LastErrorMessage = exception.Message;
                }

                var stateDataException = new StateData
                {
                    SerialNumber = _context.Mini?.SerialNumber ?? "N/A",
                    MessageType = MessageTypeException,
                    Message = null,
                    State = _context.Mini?.State ?? MiniState.Waiting,
                    Status = _context.Mini?.QboxStatus
                };
                await _stateStoreService.StoreAsync(_correlationId, stateDataException);

                _logger.LogError(exception, "SerialNumber {SerialNumber}", stateDataException.SerialNumber);

                throw;
            }
        }

        private async Task VisitPayloadsAsync(MiniParseResult miniParseResult)
        {
            var payloads = miniParseResult.Model.Payloads;

            // Loop all payloads except CounterPayload and visit
            var exceptionInformation = new ConcurrentQueue<(string details, Exception exception)>();
            foreach (var payload in payloads.Where(p => !(p is CounterPayload)))
            {
                try
                {
                    payload.Visit(this);
                }
                catch (Exception ex)
                {
                    exceptionInformation.Enqueue((payload.GetType().Name, ex));
                }
            }

            var counterDataList = new List<CounterData>();
            foreach (var counterPayload in payloads.OfType<CounterPayload>())
            {
                if (TryMapCounterPayload(miniParseResult, counterPayload, out CounterData counterData))
                {
                    counterDataList.Add(counterData);
                }
            }

            try
            {
                await _counterService.StoreAsync(_correlationId, counterDataList);
            }
            catch (Exception ex)
            {
                exceptionInformation.Enqueue((nameof(CounterPayload), ex));
            }

            // Only throw exception when all counters fail
            if (payloads.Count > 0 && exceptionInformation.Count == payloads.Count)
            {
                throw new AggregateException(exceptionInformation.Select(x => x.exception));
            }

            // Else just log as error
            foreach (var info in exceptionInformation)
            {
                _logger.LogError(info.exception, $"Error storing Payload '{info.details}'");
            }
        }

        private void BuildResult(BaseParseResult result, DateTime serverTime, ResponseType inResponseType)
        {
            // sequence number of the message received
            result.Write((byte)result.SequenceNr);

            // time in seconds since 1-1-2007
            int seconds = Convert.ToInt32(serverTime.Subtract(Epoch).TotalSeconds);
            result.Write(seconds);

            result.Write(_context.Mini.Offset);

            // SequenceNr is 0-255.
            bool canSendAutoStatusCommand = inResponseType == ResponseType.Normal && CanSendAutoStatusCommand((byte)result.SequenceNr);
            bool canSendAutoAnswerCommand = inResponseType == ResponseType.Normal;

            var responseBuilder = new QboxResponseBuilder(_context.Mini, ClientRepositories.Queue, canSendAutoStatusCommand, canSendAutoAnswerCommand);

            var commands = responseBuilder.Build();
            if (commands != null && commands.Count > 0)
            {
                result.Write(commands[0]);
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

        private Counter FindCounter(CounterPayload payload)
        {
            if (payload is CounterWithSourcePayload withSourcePayload)
            {
                var counter = _context.Mini.Counters
                    .SingleOrDefault(s => s.CounterId == payload.InternalNr &&
                                          s.Secondary != withSourcePayload.PrimaryMeter &&
                                          s.GroupId == withSourcePayload.Source);

                if (counter != null)
                {
                    return counter;
                }

                // No specific counter found, try to find counter with only internalNr
            }

            // Down grading firmware could lead to find multiple counters. InternalId is not unique from firmware revision 39
            return _context.Mini.Counters.SingleOrDefault(s => s.CounterId == payload.InternalNr);
        }

        private bool TryMapCounterPayload(BaseParseResult result, CounterPayload payload, out CounterData counterData)
        {
            counterData = null;

            if (payload.Value == ulong.MaxValue)
            {
                _logger.LogWarning("TryMapCounterPayload failed for '{SerialNumber}'. The payload value equals maximum ulong", _context.Mini.SerialNumber);
                return false;
            }

            if ((payload is R21CounterPayload) && !(payload as R21CounterPayload).IsValid)
            {
                _logger.LogWarning("TryMapCounterPayload failed for '{SerialNumber}'. The payload is of type R21CounterPayload but not valid for counter '{InternalNr}'", _context.Mini.SerialNumber, payload.InternalNr);
                return false;
            }

            if (ClientNotConnected())
            {
                // No connection with client, payload is last measured value and not the real value. First real value will be spread out over missing values
                _logger.LogWarning("TryMapCounterPayload failed for '{SerialNumber}'. There is no connection with client, data not saved", _context.Mini.SerialNumber);
                return false;
            }

            if (!MapCounterId(payload, _context.Mini))
            {
                _logger.LogWarning("TryMapCounterPayload failed for '{SerialNumber}'. Mapping the counter {InternalNr} failed", _context.Mini.SerialNumber, payload.InternalNr);
                return false;
            }

            // store the data in the payload into the corresponding counter
            var counter = FindCounter(payload);
            if (counter == null)
            {
                _logger.LogWarning("TryMapCounterPayload failed for '{SerialNumber}'. Received value for unknown counter: {InternalNr}", _context.Mini.SerialNumber, payload.InternalNr);
                return false;
            }

            if (!(result is MiniParseResult parseResult))
            {
                _logger.LogWarning("TryMapCounterPayload failed for '{SerialNumber}'. The _result is not of type MiniParseResult", _context.Mini.SerialNumber);
                return false;
            }

            // Counter is valid, set the LastDataReceived
            _context.Mini.QboxStatus.LastDataReceived = _dateTimeService.UtcNow;

            // Counter is valid, define a valid CounterData object
            counterData = new CounterData
            {
                SerialNumber = _context.Mini.SerialNumber,
                MeasureTime = parseResult.Model.MeasurementTime.ToUniversalTime(), // .ToAmsterdam(), // Change to Dutch Timezone
                CounterId = payload.InternalNr,
                PulseValue = Convert.ToInt32(payload.Value)
            };

            return true;
        }

        #region Implementation of IVisitor
        public void Accept(CounterPayload payload)
        {
            throw new NotSupportedException();
        }

        public void Accept(DeviceSettingsPayload payload)
        {
            try
            {
                if (Enum.GetValues(typeof(DeviceSettingType)).Cast<DeviceSettingType>().Contains(payload.DeviceSetting))
                {
                    string key = payload.DeviceSetting.ToString();
                    _context.Mini.QboxStatus.DebugSettings[key] = payload.DeviceSettingValueStr;
                    _context.Mini.QboxStatus.DebugSettingsLastReceived[key] = _dateTimeService.UtcNow;
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
                _logger.LogError(e, e.Message);
            }
        }
        #endregion

        /// <summary>
        /// Change the InternalId (CounterId) of the payload to the value that will be used to store and retrieve the measurements.
        /// </summary>
        /// <returns>false if the mapping could not be done (for example when the secondary meter type of a duo is not an S0 meter).</returns>
        private bool MapCounterId(CounterPayload ioPayload, Mini inMini)
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
                    _logger.LogError("Qbox {SerialNumber} has counter for unsupported secondary meter type {SecondaryMeterType}", inMini.SerialNumber,
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

        private enum ResponseType
        {
            Basic,
            Normal
        }
    }
}