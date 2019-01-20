using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Mini.R07
{
    [TestFixture]
    public class MiniR07Test
    {
        [SetUp]
        public void Init()
        {
            // Setup static logger factory
            QboxNextLogProvider.LoggerFactory = new LoggerFactory();
        }

        [Test]
        public void ParseHexStringProtocolShouldReturnParseResultTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";
            //UnitOfWorkHelper.CurrentDataStore = new ThreadContextDataStore();
            //UnitOfWorkHelper.CurrentDataStore[DataStoreName.cLogger] = QboxLogger.GetConsoleLogger("test");

            // Act
            var actual = new MiniR07().Parse(source);

            // Assert
            Assert.IsNotNull(actual);

        }

        [Test]
        public void Mini07ParseResultShouldHaveParseErrorModelTest()
        {
            // Arrange
            const string source = "0E13070B39778000410100000000";

            // Act
            var actual = new MiniR07().Parse(source) as ErrorParseResult;

            // Assert
            Assert.IsNotNull(actual);            
        }

        [Test]
        public void Mini07ParseResultShouldHaveModelOfTypeMini07ParseModelTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<Mini07ParseModel>(actual.Model);
            
        }

        [Test]
        public void Mini07ParseResultShouldIgnoreUnbalancedParenthesesTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(8.2(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsNull(actual.Model.Payloads.Cast<CounterPayload>().FirstOrDefault(d => d.InternalNr == 181));
        }

        [Test]
        public void Mini07ParseModelShouldHaveValuesSetCorrectlyTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";
            const int sequenceNr = 198;
            const int protocolNr = 1;            

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(sequenceNr, actual.SequenceNr);
            Assert.AreEqual(protocolNr, actual.ProtocolNr);
        }

        [Test]
        public void Mini07ParseModelShouldHaveStatusSetCorrectlyTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);

            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            Assert.IsTrue(model.Status.TimeIsReliable);
            Assert.IsTrue(model.Status.ValidResponse);
            Assert.AreEqual(model.Status.Status, MiniState.Operational);
        }

        [Test]
        public void Mini07ParseModelShouldHaveTimeSetCorrectlyTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";

            var expected = new DateTime(2007, 1, 1).AddSeconds(177253920);

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            
            var model = actual.Model as Mini07ParseModel;
            
            Assert.IsNotNull(model);
            Console.WriteLine(model.MeasurementTime);
            Assert.AreEqual(expected, model.MeasurementTime );
        }

        [Test]
        public void Mini07ParseModelShouldHaveCorrectMeterTypeSetTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";
            
            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);            
            Assert.AreEqual(DeviceMeterType.Smart_Meter_E, model.MeterType);
        }

        [Test]
        public void Mini07ParseModelShouldHaveCorrectPayloadIndicatorSetTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);
            
            Assert.IsTrue(model.PayloadIndicator.SmartMeterIsPresent);
            Assert.IsFalse(model.PayloadIndicator.DeviceSettingPresent);
        }

        [Test]
        public void Mini07ParseShouldContainSmartMeterPayloadTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00000.000*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(4, model.Payloads.Count);
            
        }

        [Test]
        public void Mini07ParseShouldContainDeviceSettingsPayloadTest()
        {
            // Arrange
            const string source = "01C6070A90AE2006400500000050";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Single(s => s.GetType() == typeof (DeviceSettingsPayload)) as DeviceSettingsPayload;
            Assert.IsNotNull(deviceSettings);
            Assert.AreEqual("80", deviceSettings.DeviceSettingValueStr);
        }


        [Test]
        public void Mini07ParseShouldContainDeviceSettingSensor1LowPayloadTest()
        {
            // Arrange
            const string source = "0E06070B0B1E80024206088B01000000550300000000";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Single(s => s.GetType() == typeof(DeviceSettingsPayload)) as DeviceSettingsPayload;
            Assert.IsNotNull(deviceSettings);
            Assert.AreEqual((0x088B).ToString(), deviceSettings.DeviceSettingValueStr);
            Assert.AreEqual(DeviceSettingType.Sensor1Low, deviceSettings.DeviceSetting);
        }



        [Test]
        public void Mini07ParseShouldContainDeviceSettingSensor1LowPayloadUnsignedInt16Test()
        {
            // Arrange
            const string source = "0E06070B0B1E80024206F88B01000000550300000000";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Single(s => s.GetType() == typeof(DeviceSettingsPayload)) as DeviceSettingsPayload;
            Assert.IsNotNull(deviceSettings);
            Assert.AreEqual((0xF88B).ToString(), deviceSettings.DeviceSettingValueStr);
            Assert.AreEqual(DeviceSettingType.Sensor1Low, deviceSettings.DeviceSetting);
        }


        [Test]
        public void Mini07ParseShouldContainDeviceSettingClientPrimaryMeterTypeWithoutClientBytePayloadTest()
        {
            // Arrange
            const string source = "2006070B0B1E800242410201000000550300000000";
            //                     1 2 3 4       5 6 7   8
            // 1 Protocol nr
            // 2 Sequence nr
            // 3 Device status
            // 4 Time
            // 5 Primary metertype
            // 6 Payload indicator
            // 7 DeviceSetting
            // 8 Counter payload

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Single(s => s.GetType() == typeof(DeviceSettingsPayload)) as DeviceSettingsPayload;
            Assert.IsNotNull(deviceSettings);
            Assert.IsTrue(deviceSettings.DeviceSetting == DeviceSettingType.ClientPrimaryMeterType);
            Assert.IsTrue(deviceSettings.Client == null);
            Assert.IsTrue(deviceSettings.DeviceSettingValueStr == "2");
        }

        [Test]
        public void Mini07ParseShouldContainDeviceSettingClientPrimaryMeterTypeWithClientBytePayloadTest()
        {
            // Arrange
            const string source = "2706070B0B1E80024241010201000000550300000000";
            //                     1 2 3 4       5 6 7     8
            // 1 Protocol nr
            // 2 Sequence nr
            // 3 Device status
            // 4 Time
            // 5 Primary metertype
            // 6 Payload indicator
            // 7 DeviceSetting
            // 8 Counter payload

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Single(s => s.GetType() == typeof(DeviceSettingsPayload)) as DeviceSettingsPayload;
            Assert.IsNotNull(deviceSettings);
            Assert.IsTrue(deviceSettings.DeviceSetting == DeviceSettingType.ClientPrimaryMeterType);
            Assert.IsTrue(deviceSettings.Client == QboxClient.Client1);
            Assert.IsTrue(deviceSettings.DeviceSettingValueStr == "2");
        }

        [Test]
        public void Mini07ParseShouldContainDeviceSettingCalibrationSettingsTest()
        {
            // Arrange, voorbeeld response uit qplat-52
            const string source = "0F04070B1CCE6400410A06FFFF07FFFF08FFFF09FFFF0100000000";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Where(s => s.GetType() == typeof(DeviceSettingsPayload)).Cast<DeviceSettingsPayload>().ToList();
            Assert.IsNotNull(deviceSettings);
            Assert.IsTrue(deviceSettings.Count() == 4, "4 device settings expected");
            var sum = deviceSettings.Select(d => (int)d.DeviceSetting).Sum();
            Assert.IsTrue(sum == 30);

            var counterPayloads = model.Payloads.Where(s => s.GetType() == typeof(CounterPayload)).Cast<CounterPayload>().ToList();
            Assert.IsTrue(counterPayloads.Count() == 1, "1 counter payload expected");
            Assert.IsTrue(counterPayloads.First().InternalNr == 1);
            Assert.IsTrue(counterPayloads.First().Value == 0);
        }

        [Test]
        public void Mini07ParseShouldContainDeviceSettingLEDSensorSettingsTest()
        {
            // Arrange, voorbeeld response uit qplat-52
            const string source = "0F04070B1CCE6400412821FFFF22FFFF23FFFF24FFFF25FFFF26FFFF27FFFF0100000000";// "0E06070B0B1E8002422801000001111101222201333301444401555501666601000000550300000000";
                                                       
            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Where(s => s.GetType() == typeof(DeviceSettingsPayload)).Cast<DeviceSettingsPayload>().ToList();
            Assert.IsNotNull(deviceSettings);
            Assert.IsTrue(deviceSettings.Count() == 7, "7 device settings expected");
            var sum = deviceSettings.Select(d => (int)d.DeviceSetting).Sum();
            Assert.IsTrue(sum == 252);

            var counterPayloads = model.Payloads.Where(s => s.GetType() == typeof(CounterPayload)).Cast<CounterPayload>().ToList();
            Assert.IsTrue(counterPayloads.Count() == 1, "1 counter payload expected");
            Assert.IsTrue(counterPayloads.First().InternalNr == 1);
            Assert.IsTrue(counterPayloads.First().Value == 0);
        }

         
        [Test]
        public void Mini07ParseShouldContainDeviceSettingLEDSensorMeasurementsTest()
        {
            // Arrange, voorbeeld response uit qplat-53
            const string source = "0F2B070B1CCB94004132290FFF2A00002B0FFF2C00002D00002E00002F00003000003100000100000000";
            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var deviceSettings = model.Payloads.Where(s => s.GetType() == typeof(DeviceSettingsPayload)).Cast<DeviceSettingsPayload>().ToList();
            Assert.IsNotNull(deviceSettings);
            Assert.IsTrue(deviceSettings.Count() == 9, "9 device settings expected");
            var sum = deviceSettings.Select(d => (int)d.DeviceSetting).Sum();
            Assert.IsTrue(sum == 405);

            var counterPayloads = model.Payloads.Where(s => s.GetType() == typeof(CounterPayload)).Cast<CounterPayload>().ToList();
            Assert.IsTrue(counterPayloads.Count() == 1, "1 counter payload expected");
            Assert.IsTrue(counterPayloads.First().InternalNr == 1);
            Assert.IsTrue(counterPayloads.First().Value == 0);
        }

        [Test]
        public void Mini07ParseShouldContainCounterValuesPayloadTest()
        {
            // Arrange
            const string source = "01C6070A90AE200602010000ff1203000f0023";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);

            var counterValue = model.Payloads.Cast<CounterPayload>().Single(s => s.InternalNr == 1);
            Assert.IsNotNull(counterValue);
            Assert.AreEqual((ulong)65298, counterValue.Value);
            counterValue = model.Payloads.Cast<CounterPayload>().Single(s => s.InternalNr == 3);
            Assert.AreEqual((ulong)983075, counterValue.Value);

        }

        [Test]
        public void WhenMiniDumpsSmartMeterE_GItShouldParseTheGasTest()
        {
            const string source = @"0897070A9D77F00780/ISk5\2ME382-1003 0-0:96.1.1(4B414C37303035303632363737303131) 1-0:1.8.1(00996.884*kWh) 1-0:1.8.2(00910.201*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.64*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(0999.00*kW) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() 0-1:24.1.0(3) 0-1:96.1.0(3238303039303031313338303931393131) 0-1:24.3.0(120823060000)(00)(60)(1)(0-1:24.2.1)(m3) (00218.626) 0-1:24.4.0(1) !";
            
             // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<MiniParseModel>(actual.Model);
            Assert.AreEqual(5, actual.Model.Payloads.Count);
            var counterPayload = actual.Model.Payloads[4] as CounterPayload;
            if (counterPayload != null)
                Assert.AreEqual((ulong)218626, counterPayload.Value);
        }

        [Test]
        public void WhenMiniDumpsSmartMeterE_GItShouldParseTheGasAndIgnoreGasPayloadWithoutUnitTest()
        {
            const string source = @"0EAE070BD33A8C0680/ISk5\2MT382-1003    0-0:96.1.1(5A424556303035303933303339343132)  1-0:1.8.1(02108.875*kWh)  1-0:1.8.2(02557.269*kWh)  1-0:2.8.1(00000.000*kWh)  1-0:2.8.2(00000.002*kWh)  0-0:96.14.0(0001)  1-0:1.7.0(0001.08*kW)  1-0:2.7.0(0000.00*kW)  0-0:17.0.0(0999.00*kW)  0-0:96.3.10(1)  0-0:96.13.1()  0-0:96.13.0()  0-1:24.1.0(3)  0-1:96.1.0(3234313537303032393638303333313132)  0-1:24.3.0(130415050000)(00)(60)(1)(0-1:24.2.1)()  (00000000)  0-1:24.4.0(1)  0-2:24.1.0(3)  0-2:96.1.0(3234313537303032393637393734363132)  0-2:24.3.0(130415050000)(00)(60)(1)(0-2:24.2.1)(m3)  (04167.080)  0-2:24.4.0(1)  !";

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<MiniParseModel>(actual.Model);
            Assert.AreEqual(5, actual.Model.Payloads.Count);
            var counterPayload = actual.Model.Payloads.Cast<CounterPayload>().FirstOrDefault(d => d.InternalNr == 2421);
            if (counterPayload != null)
                Assert.AreEqual((ulong)4167080, counterPayload.Value);
        }

        [Test]
        public void WhenMiniDumpsSoladinItShouldBeParsedTest()
        {
            const string source = @"0978070AADE9F4098000001100B6F30000C401B2018A13E8000000B1005009003B443A000000007A";

            // act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // assert
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Model as Mini07ParseModel);
            Assert.AreEqual(1, actual.Model.Payloads.Count);
            var counterPayload = actual.Model.Payloads[0] as CounterPayload;
            if (counterPayload != null)
                Assert.AreEqual((ulong)2384, counterPayload.Value);
        }

        [Test] public void WhenMiniDumpsSoladinWithoutPayloadItShouldReturnOkTest()
        {
            // arrange
            const string source = @"09E0050AB4A8D40980";

            // act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // assert
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Model);
            Assert.IsNotNull(actual.Model.Payloads);
            Assert.IsFalse(actual.Model.Payloads.Any());
            Assert.AreEqual(224, actual.SequenceNr);
        }

        [Test]
        public void Mini07ParseShouldContainClientStatusPayloadTest()
        {
            // Arrange
            const string source = "201C070B5BA5A80012011D01000000000100000000";
            // In the above example (report from QBox), the hex-byte “12” represents the Payload indication, indicating that there is one or more Client Status(Bit 4). 
            // The following byte “01” indicates that there is one Meterkast QBox ‘s status present, which is “1D”, in this case. 

            // Act
            var actual = new MiniR07().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            var model = actual.Model as Mini07ParseModel;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.PayloadIndicator.ClientStatusPresent);

            var clientStatuses = model.Payloads.OfType<ClientStatusPayload>();
            Assert.IsNotNull(clientStatuses);
            Assert.IsTrue(clientStatuses.Count() == 1);
        }


		[Test]
		public void QboxDumpWithLEDMeterTypeAndSmartMeterIsPresentBitSetAndUnreliableTimestampTest()
		{
			var source = "28000500000000018248FB08013F30002000FB7420000000092F20001E2CA1000000080100000001082000001A30000000050500080193F500400138000000000000F52000112C9300000000750D000000B4FF200020001E2000D108B420000A1C002000C748";
			Assert.IsTrue(new MiniR07().Parse(source) is ErrorParseResult);
		}


		[Test]
		public void QboxDumpWithTimestampInFeatureAndUnknownMeterType2Test()
		{
			var source = "2820052000FB482F0C2C11000000030008012D5F2020000004000000000000000000030020FF0D770000000002030000000000200001010020000801000000712C5F20000A1C";
			Assert.IsTrue(new MiniR07().Parse(source) is ErrorParseResult);
		}


    }

}
