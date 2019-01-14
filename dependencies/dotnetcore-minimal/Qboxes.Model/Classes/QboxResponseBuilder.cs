using NLog;
using Qboxes.Utils;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Protocols;

namespace Qboxes.Classes
{
	/// <summary>
	/// QboxResponseBuilder builds a list of commands to send back to the Qbox.
	/// </summary>
	/// <remarks>
	/// The list of commands is built in the following order:
	/// - auto answer
	/// - command queue (commands given in Qmanager)
	/// - auto status
	/// The first commands found will cause the other commands to be ignored.
	/// So when there is an auto answer available, nothing will be read from the command queue and the auto status will be skipped.
	/// When there is no auto answer availabe, but there is a command waiting in the command queue, it will be retrieved, but the auto status will be skipped.
	/// In that case only one command will be read from the queue.
	/// </remarks>
	public class QboxResponseBuilder
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public QboxResponseBuilder(MiniPoco inMini, IQueue<string> ioCommandQueue, bool inCanSendAutoStatusCommands, bool inCanSendAutoAnswerCommand)
		{
			_mini = inMini;
			_commandQueue = ioCommandQueue;
			_canSendAutoStatusCommands = inCanSendAutoStatusCommands;
			_canSendAutoAnswerCommands = inCanSendAutoAnswerCommand;
		}

		
		/// <summary>
		/// Build the list of commands to send to the Qbox.
		/// </summary>
		public List<string> Build()
		{
			List<string> commands = GetAutoAnswerCommands();

			if (commands == null || commands.Count == 0)
			{
				var command = GetQueueCommand();
				if (!string.IsNullOrEmpty(command))
					commands = new List<string> { command };
			}

			if (commands == null || commands.Count == 0)
				commands = GetAutoStatusCommands();

			return commands;
		}


		/// <summary>
		/// Get the command that is in front of the command queue.
		/// </summary>
		/// <returns>The command or null when there is no command waiting.</returns>
		private string GetQueueCommand()
		{
			if (_commandQueue == null)
				return null;

			try
			{
				var command = _commandQueue.Dequeue(_mini.SerialNumber);
				_log.Info("{0} DeQueue Command: {1}", _mini.SerialNumber, command ?? "null");
				return command;
			}
			catch (Exception e)
			{
				_log.Error(e, e.Message);
				return null;
			}
		}


		/// <summary>
		/// Get the commands that are the result of the auto answer mechanism.
		/// </summary>
		private List<string> GetAutoAnswerCommands()
		{
			if (!_mini.AutoAnswer || !_canSendAutoAnswerCommands)
				return null;

			List<string> commands = GetAutoAnswerCommandsForHost();
			commands.AddRange(GetAutoAnswerCommandsForClients());
			return commands;
		}


		/// <summary>
		/// Get the auto answer commands for the host Qbox.
		/// </summary>
		private List<string> GetAutoAnswerCommandsForHost()
		{
			var commandsToCombine = new List<string>();
			var separateCommands = new List<string>();

			// Host qbox
			switch (_mini.State)
			{
				case MiniState.HardReset:
					break;
				case MiniState.Waiting:
					// Host metertype
					commandsToCombine.Add(Mini.WriteMeterType(_mini.MeterType));

					if (_mini.SecondaryMeterType != DeviceMeterType.None)
						commandsToCombine.Add(Command.Encode((byte)DeviceSettingType.SecondaryMeterType, (byte)_mini.SecondaryMeterType));

					if (_mini.MeterType != DeviceMeterType.NO_Meter)
					{
						// Last known meter settings send to qbox
						var meterSettings = _mini.MeterSettings;
						// Default metersettings in case no last meter settings are found
						if (String.IsNullOrEmpty(meterSettings))
							meterSettings = Command.DefaultMeterSettings(_mini.MeterType, QboxClient.None);

						// Since the meter settings may contain commands that can not be safely combined with other commands,
						// for example set meter type to led + set led channel, we put the meter settings in separate command.
						if (!String.IsNullOrEmpty(meterSettings))
							separateCommands.Add(meterSettings);
					}

					foreach (var client in _mini.Clients)
						commandsToCombine.Add(Command.Encode((byte)ClientActivityRequest.RequestToRestartClientWithFactoryDefaults, client.ClientId));

					break;
				case MiniState.ValidImage:
					break;
				case MiniState.InvalidImage:
					commandsToCombine.Add(Command.Encode(Command.RequestUpgradeFirmware));
					break;
			}

			var commands = new List<string>();
			if (commandsToCombine.Count > 0)
				commands.Add(String.Join(String.Empty, commandsToCombine));
			if (separateCommands.Count > 0)
				commands.AddRange(separateCommands);

			return commands;
		}


		/// <summary>
		/// Get the auto answer commands for the client Qboxes.
		/// </summary>
		private List<string> GetAutoAnswerCommandsForClients()
		{
			var commandsToCombine = new List<string>();
			var separateCommands = new List<string>();

			// NOTE: qplat-89, zie comment hierboven
			// Client qbox(en)
			foreach (var client in _mini.Clients)
			{
				var clientState = _mini.GetClientMiniStatus(client);
				if (ClientAutoCommandsAllowed(clientState))
				{
					if (_mini.State == MiniState.ValidImage)
					{
						commandsToCombine.Add(Command.Encode((byte)ClientActivityRequest.ReplicateFirmwareToClient, client.ClientId));
					}
					else
					{
						if (clientState != null)
						{
							if (clientState.ActionsFailed)
							{
								_log.Warn("Actions failed on client {0} of Qbox {1}", client.ClientId, _mini.SerialNumber);
								// Refactor: prior(last) command must be send, not necessarily ResetFactoryDefaults
								// This command is not available at the moment, value must be stored.
								// 28-05-13: Uitgecommentarieerd, Resulteert nu in een A200 loop
								//_result.Write((byte)ClientActivityRequest.RequestToRestartClientWithFactoryDefaults);
								//_result.Write((byte)client.ClientId);
							}
							else
							{
								switch (clientState.State)
								{
									case ClientState.HardReset:
										break;
									case ClientState.WaitingForMeterType:
										commandsToCombine.Add(Command.Encode((byte)DeviceSettingType.ClientPrimaryMeterType, client.ClientId, (byte)client.MeterType));

										if (client.SecondaryMeterType != DeviceMeterType.None)
											commandsToCombine.Add(Command.Encode((byte)DeviceSettingType.ClientSecondaryMeterType, client.ClientId, (byte)client.SecondaryMeterType));

										if (_mini.MeterType == DeviceMeterType.NO_Meter)
										{
											// Use last known meterSettings
											var meterSettings = _mini.MeterSettings;
											if (String.IsNullOrEmpty(meterSettings))
											{
												// No meterSettings available, use default metersettings:
												meterSettings = Command.DefaultMeterSettings(client.MeterType, (QboxClient)client.ClientId);
											}
											if (!String.IsNullOrEmpty(meterSettings))
												separateCommands.Add(meterSettings);
										}
										break;
									case ClientState.ValidImageDownloaded:
										// reset factory default after download firmware
										commandsToCombine.Add(Command.Encode(Command.RequestRestartWithFactoryDefaults));
										break;
									case ClientState.InvalidImageDownloaded:
										commandsToCombine.Add(Command.Encode(Command.RequestUpgradeFirmware));
										break;
								}
							}
						}
					}
				}
			}

			var commands = new List<string>();
			if (commandsToCombine.Count > 0)
				commands.Add(String.Join(String.Empty, commandsToCombine));
			if (separateCommands.Count > 0)
				commands.AddRange(separateCommands);

			return commands;
		}


		/// <summary>
		/// Get the command string that asks for device or client settings that we want to retrieve as often as configured.
		/// Which device setting is retrieved is dependent on the type of the Qbox (mono/duo, meter type).
		/// </summary>
		private List<string> GetAutoStatusCommands()
		{
			var commands = new List<string>();

			if (!_canSendAutoStatusCommands)
				return commands;

			if (IsLedMeter(_mini))
				commands.Add(Command.Encode(Command.RequestDeviceSetting, (byte)DeviceSettingType.SensorChannel));
			else if (IsFerrarisMeter(_mini))
				commands.Add(Command.Encode(Command.RequestDeviceSetting, (byte)DeviceSettingType.CalibrationSettingsComposite));	// Ferraris callibration settings for low value sensor 1, high value sensor 1, low value sensor 2 and high value sensor 2

			if (IsDuo(_mini))
			{
				var clientState = _mini.GetClientMiniStatus(_mini.Clients.FirstOrDefault());
				if (ClientAutoCommandsAllowed(clientState))
				{
					commands.Add(Command.Encode((byte)DeviceSettingType.ClientActivityRequestProductAndSerialNumber, 0));	// Request to report product and serial number (PN/SN) from client 0.
					commands.Add(Command.Encode((byte)DeviceSettingType.ClientActivityRequestFirmwareVersion, 0));	// Request to report firmware version from client 0.	
				}
			}

			return commands;
		}


		/// <summary>
		/// Is inMini a LED meter?
		/// </summary>
		private bool IsLedMeter(MiniPoco inMini)
		{
			return inMini.IsMeterTypePresent(DeviceMeterType.LED_TypeI) || inMini.IsMeterTypePresent(DeviceMeterType.LED_TypeII);
		}


		/// <summary>
		/// Is inMini a Ferraris meter?
		/// </summary>
		private bool IsFerrarisMeter(MiniPoco inMini)
		{
			return inMini.IsMeterTypePresent(DeviceMeterType.Ferraris_Black_Toothed);
		}


		/// <summary>
		/// Is inMini a duo meter?
		/// </summary>
		private bool IsDuo(MiniPoco inMini)
		{
			return inMini.Clients.Any();
		}


		/// <summary>
		/// Auto commands allowed for given client based on ClientState
		/// </summary>
		private bool ClientAutoCommandsAllowed(ClientMiniStatus inClient)
		{
			return (inClient == null) || ((inClient.State != ClientState.Unknown) && (inClient.State != ClientState.UpgradeInProgress));
		}


		private static Logger _log = LogManager.GetLogger("QboxResponseBuilder");
		private MiniPoco _mini;
		private IQueue<string> _commandQueue;
		private bool _canSendAutoStatusCommands;
		private bool _canSendAutoAnswerCommands;
	}
}
