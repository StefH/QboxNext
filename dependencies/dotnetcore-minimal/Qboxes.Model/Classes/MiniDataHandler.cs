using System;
using System.Linq;
using NLog;
using QboxNext.Core.Dto;
using QboxNext.Core.Log;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Interfaces;
using System.Collections.Generic;
using Qboxes.Interfaces;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Factories;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Model;

namespace Qboxes.Classes
{
    /// <summary>
    /// Class that encapsulates the handling of the Qbox mini data dump
    /// </summary>
    public class MiniDataHandler: IVisitor
    {
        private static readonly Logger Log = QboxNextLogFactory.GetLogger("MiniDataHandler");
		private static readonly Dictionary<int, int> SmartMeterIdMapping = new Dictionary<int, int>()
			                                                                   {
				                                                                   { 1, 181 },
																				   { 2, 182 },
																				   { 3, 281 },
																				   { 4, 282 },
																				   { 5, 270 },
																				   { 6, 170 },
																				   { 7, 2421 }
			                                                                   };
		private static readonly Dictionary<int, int> SoladinIdMapping = new Dictionary<int, int>()
			                                                                   {
				                                                                   { 3, 120 },
																			   };
		private static readonly DateTime Epoch = new DateTime(2007, 1, 1);
		private readonly QboxDataDumpContext _context;
        private BaseParseResult _result;
	    private IQboxMessagesLogger _qobxMessagesLogger;

	    public static List<int> AutoStatusCommandSequenceNrs { get; set; }


        public MiniDataHandler(QboxDataDumpContext context, IQboxMessagesLogger qboxMessagesLogger)
        {
            Guard.IsNotNull(context, "context is missing");
            _context = context;
	        _qobxMessagesLogger = qboxMessagesLogger;
        }

        public string DebugTrace()
        {
            using (new ExtendedLogger(_context.Mini.SerialNumber))
            {
				LogQboxMessage(_context.Message, QboxMessageType.Trace);
                return ""; // Niet duidelijk of er iets terug gestuurd moet worden
            }
        }

        public string Handle()
        {        
            using (new ExtendedLogger(_context.Mini.SerialNumber))
            {
                Log.Trace("Enter");
                try
                {
                    Log.Info("sn: {0} | input: {1} | lastUrl: {2}", _context.Mini.SerialNumber, _context.Message,
                              _context.Mini.QboxStatus.Url);

					LogQboxMessage(_context.Message, QboxMessageType.Request);

                    // start parsing
                    
                    var parser = ParserFactory.GetParserFromMessage(_context.Message);
                    _result = parser.Parse(_context.Message);
                    
                    // end of parsing

                    if ((_result as MiniParseResult) != null)
                    {
                        var parseResult = (_result as MiniParseResult);

                        // handle the result
						_context.Mini.QboxStatus.FirmwareVersion = parseResult.ProtocolNr;
                        _context.Mini.State = parseResult.Model.Status.Status;
                        _context.Mini.QboxStatus.State = (byte)parseResult.Model.Status.Status;
                        var operational = false;
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
                            _context.Mini.QboxStatus.LastNotOperational = DateTime.UtcNow;
                        
                        if (parseResult.Model.Status.TimeIsReliable)
                            _context.Mini.QboxStatus.LastTimeIsReliable = DateTime.UtcNow;
                        else 
                            _context.Mini.QboxStatus.LastTimeUnreliable = DateTime.UtcNow;

                        if (parseResult.Model.Status.ValidResponse)
                            _context.Mini.QboxStatus.LastValidResponse = DateTime.UtcNow;
                        else
                            _context.Mini.QboxStatus.LastInvalidResponse = DateTime.UtcNow;

                        foreach (var payload in parseResult.Model.Payloads)
                            payload.Visit(this);

						BuildResult(ResponseType.Normal);
					}
                    else
                    {
                        var errorParseResult = _result as ErrorParseResult;
						if (errorParseResult != null)
							LogQboxMessage(errorParseResult.Error, QboxMessageType.Error);

						// We could not handle the message normally, but if we don't answer at all, the Qbox will just retransmit the message.
						// So we just send back the basic message, without handling the queue and auto-answer.
						BuildResult(ResponseType.Basic);
					}
                   
                    _context.Mini.QboxStatus.LastSeen = DateTime.UtcNow;

                    Log.Debug("sn: {0} | result: {1}", _context.Mini.SerialNumber, _result.GetMessage());
					
					LogQboxMessage(_result.GetMessageWithEnvelope(), QboxMessageType.Response);

                    return _result.GetMessageWithEnvelope();
                }
                catch (Exception e)
                {
                    if (_context.Mini != null)
                    {
                        _context.Mini.QboxStatus.LastSeen = DateTime.UtcNow;
                        _context.Mini.QboxStatus.LastError = DateTime.UtcNow;
                        _context.Mini.QboxStatus.LastErrorMessage = e.Message;
                    }
					LogQboxMessage(e.ToString(), QboxMessageType.Exception);
                    Log.Error(e, String.Format("sn: {0} | Error: {1}", _context.Mini.SerialNumber, e.Message));
                    return e.Message;
                }
            }
        }


        private void BuildResult(ResponseType inResponseType)
        {
            // sequence number of the message received
            _result.Write((byte) _result.SequenceNr);

            // time in seconds from 1-1-2007
            var seconds = Convert.ToInt32((DateTime.Now.Subtract(Epoch)).TotalSeconds);
            _result.Write(seconds);

			_result.Write((byte)_context.Mini.Offset);

			// SequenceNr is 0-255.
			bool canSendAutoStatusCommand = (inResponseType == ResponseType.Normal) && CanSendAutoStatusCommand((byte)_result.SequenceNr);
			bool canSendAutoAnswerCommand = inResponseType == ResponseType.Normal;
			var responseBuilder = new QboxResponseBuilder(_context.Mini, ClientRepositories.Queue, canSendAutoStatusCommand, canSendAutoAnswerCommand);
			List<string> commands = responseBuilder.Build();

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
				return;

			foreach (var command in inCommands)
				ClientRepositories.Queue.Enqueue(_context.Mini.SerialNumber, command);
		}


        /// <summary>
        /// result = true when Mini has 1 client and clientstatus 'ConnectionWithClient' = false
        /// Check with max. 1 client, due to the missing client info in payload (duo)
        /// </summary>
        /// <returns></returns>
        private bool ClientNotConnected()
        {
            if (_context.Mini.QboxStatus.ClientStatuses.Count() == 1)
            {
				var state = new ClientMiniStatus(_context.Mini.QboxStatus.ClientStatuses.First().Value, _context.Mini.QboxStatus.FirmwareVersion);
                return !state.ConnectionWithClient;
            }
            return false;
        }

        private CounterPoco FindCounter(CounterPayload payload)
        {
            if (payload is CounterWithSourcePayload)
            {
                var pl = payload as CounterWithSourcePayload;
                var counter = _context.Mini.Counters.SingleOrDefault(s => s.CounterId == payload.InternalNr && s.Secondary != pl.PrimaryMeter && s.GroupId == pl.Source);

                if (counter != null)
                    return counter;

                // No specific counter found, try to find counter with only internalNr
            }
                
            // Down grading firmware could lead to find multiple counters. InternalId is not unique from firmware revision 39
            return _context.Mini.Counters.SingleOrDefault(s => s.CounterId == payload.InternalNr);
        }


        #region Implementation of IVisitor

		/// <summary>
		/// Process the payload
		/// </summary>
		/// <param name="ioPayload">Payload, InternalNr will be mapped to the actual internal number used to store the data.</param>
        public void Accept(CounterPayload ioPayload)
        {
            try
            {
                if (ioPayload.Value == ulong.MaxValue) 
                    return;

				if ((ioPayload is R21CounterPayload) && !(ioPayload as R21CounterPayload).IsValid)
				{
					Log.Debug("Invalid value for counter {0} / {1}", ioPayload.InternalNr, _context.Mini.SerialNumber);
					return;
				}

                if (ClientNotConnected())
                {
                    // No connection with client, payload is last measured value and not the real value. First real value will be spread out over missing values
                    Log.Debug("No connection with client, data not saved");
                    return;
                }

				if (!MapCounterId(ioPayload, _context.Mini))
					return;

                // store the data in the payload into the corresponding counter
                var counter = FindCounter(ioPayload);
                if (counter == null)
                {
					Log.Warn(string.Format("Recieved value for unknown counter: {0} / {1}", ioPayload.InternalNr, _context.Mini.SerialNumber));
					return; //todo: investigate if exception would be better 
				}

                var parseResult = _result as MiniParseResult;
                if (parseResult == null)
					return;

	            counter.SetValue(parseResult.Model.MeasurementTime, ioPayload.Value, _context.Mini.QboxStatus);
				_context.Mini.QboxStatus.LastDataReceived = DateTime.UtcNow;
			}
            catch (Exception e)
            {
                Log.Error(e, e.Message);                
            }
        }

        public void Accept(DeviceSettingsPayload payload)
        {
            try
            {
                if (Enum.GetValues(typeof(DeviceSettingType)).Cast<DeviceSettingType>().Contains(payload.DeviceSetting))
                {
					var key = payload.DeviceSetting.ToString();
                    _context.Mini.QboxStatus.DebugSettings[key] = payload.DeviceSettingValueStr;
					_context.Mini.QboxStatus.DebugSettingsLastReceived[key] = DateTime.UtcNow;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        public void Accept(ClientStatusPayload payload)
        {
            try
            {
                // qplat-74 implementation
                var client = (QboxClient)payload.Client;
                _context.Mini.QboxStatus.ClientStatuses[client.ToString()] = payload.RawValue;

                _context.Mini.QboxStatus.ClientStateDates[String.Format("{0}-{1}", client.ToString(), payload.State.State.ToString())] = payload.MeasurementTime;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        #endregion


		/// <summary>
		/// Change the InternalId (CounterId) of the payload to the value that will be used to store and retrieve the measurements.
		/// </summary>
		/// <returns>false if the mapping could not be done (for example when the secondary meter type of a duo is not an S0 meter).</returns>
		public static bool MapCounterId(CounterPayload ioPayload, MiniPoco inMini)
		{
			Dictionary<int, int> mapping = null;

			var counterWithSourcePayload = ioPayload as CounterWithSourcePayload;

			if (counterWithSourcePayload == null || counterWithSourcePayload.PrimaryMeter)
			{
				// Firmware A34, Smart Meter and soladin measured on client reports counters instead of specific message:
				if (inMini.Clients.Any(d => (d.MeterType == DeviceMeterType.Smart_Meter_E) || (d.MeterType == DeviceMeterType.Smart_Meter_EG)))
					mapping = SmartMeterIdMapping;
				else if (inMini.Clients.Any(d => d.MeterType == DeviceMeterType.Soladin_600))
					mapping = SoladinIdMapping;
			}

			if (counterWithSourcePayload != null && !counterWithSourcePayload.PrimaryMeter)
			{
				// At the moment we only support S0 as secondary meter type, so log an error when we find a different meter type.
				var clientsWithUnsupportedSecondaryMeterTypes = inMini.Clients.Where(c => c.SecondaryMeterType != DeviceMeterType.None &&
																						  c.SecondaryMeterType != DeviceMeterType.SO_Pulse).ToList();
				if (clientsWithUnsupportedSecondaryMeterTypes.Count > 0)
				{
					Log.Error("Qbox {0} has counter for unsupported secondary meter type {1}", inMini.SerialNumber,
							clientsWithUnsupportedSecondaryMeterTypes[0].SecondaryMeterType);
					return false;
				}

				// S0 as secondary meter type does not need to be mapped.
			}

			if (mapping != null && mapping.ContainsKey(ioPayload.InternalNr))
				ioPayload.InternalNr = mapping[ioPayload.InternalNr];

			return true;
		}


		/// <summary>
		/// Can we send the auto status commands on the specified sequence nr?
		/// </summary>
		private bool CanSendAutoStatusCommand(byte inSequenceNr)
		{
			// If no sequence nrs have been set, we don't send auto commands at all.
			if (AutoStatusCommandSequenceNrs == null)
				return false;

			return (AutoStatusCommandSequenceNrs.Contains(inSequenceNr));
		}


		private void LogQboxMessage(string message, QboxMessageType messageType)
		{
			_qobxMessagesLogger.LogQboxMessage(_context.Mini.SerialNumber, message, messageType);
		}


		private enum ResponseType
		{
			Basic,
			Normal
		}
	}
}
