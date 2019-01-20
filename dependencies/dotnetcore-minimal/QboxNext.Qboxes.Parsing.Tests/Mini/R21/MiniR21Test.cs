using System.Linq;
using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Mini.R21
{
    [TestFixture]
    public class MiniR21Test
    {
		/// <summary>
		/// ClientStatus is changed in from R17 -> R21, extra byte (ClientId) is added
		/// </summary>
		[Test]
		public void MiniR21ClientStatusTest()
		{
            // Arrange
            const string source = "2BAA110DA4D7E91E10010005";
			//                     1 2 3 4       5 6 7 
			// 1 Protocol nr
			// 2 Sequence nr
			// 3 Device status
			// 4 Time
			// 5 Primary metertype
			// 6 Payload indicator
			// 7 CLient Status = #status entries, client ID, status

			// Act
			var actual = new MiniR21().Parse(source) as MiniParseResult;

			// Assert
			Assert.IsNotNull(actual);
			Assert.AreEqual(0x05, actual.Model.Payloads.Cast<ClientStatusPayload>().FirstOrDefault().RawValue); 
		}


		[Test]
		public void MiniR21ClientStatusWithClientId2Test()
		{
            // Arrange
            const string source = "2BAB070DA4D8401E10010221";
			//                     1 2 3 4       5 6 7 
			// 1 Protocol nr
			// 2 Sequence nr
			// 3 Device status
			// 4 Time
			// 5 Primary metertype
			// 6 Payload indicator
			// 7 CLient Status

			// Act
			var actual = new MiniR21().Parse(source) as MiniParseResult;

			// Assert
			Assert.IsNotNull(actual);
			Assert.AreEqual(0x02, actual.Model.Payloads.Cast<ClientStatusPayload>().FirstOrDefault().Client);
			Assert.AreEqual(0x21, actual.Model.Payloads.Cast<ClientStatusPayload>().FirstOrDefault().RawValue); 
		}


		[Test]
		public void R21ParseHexStringProtocolShouldReturnParseResultTest()
		{
			// Arrange
			const string source = "2B06070DA4ED601E464D470101\"00450144F169FFFF4602\"600102318601000ABF4A0300043827020000000004000000000500000000060000001E";
			//                                       ^ 4D = Start Composite Zwave

			// Act
			var actual = new MiniR21().Parse(source) as MiniParseResult;

			// Assert
			Assert.IsNotNull(actual);
			var counterPayloads = actual.Model.Payloads.Where(d => d is R21CounterPayload).Cast<R21CounterPayload>().ToList();
			// Check if all counterpayloads are valid
			Assert.IsFalse(counterPayloads.Where(d => !d.IsValid).Any());
			var deviceSettings = actual.Model.Payloads.Where(d => d is DeviceSettingsPayload).Cast<DeviceSettingsPayload>().ToList();
			Assert.AreEqual(1, deviceSettings.Count());
		}

		[Test]
		public void R21ParseHexStringProtocolWithInvalidCounterValueTest()
		{
			// Arrange
			const string source = "2B06070DA4ED601E464D470101\"00450144F169FFFF4602\"600102318601000ABF4A03000438270200000000040000000005000000008600000000";
			//                                       ^ 4D = Start Composite Zwave                                                                ^ invalid value for counter 6

			// Act
			var actual = new MiniR21().Parse(source) as MiniParseResult;

			// Assert
			Assert.IsNotNull(actual);
			var counterPayloads = actual.Model.Payloads.Where(d => d is R21CounterPayload).Cast<R21CounterPayload>().ToList();
			// Counter 6 is invalid
			Assert.AreEqual(6, counterPayloads.FirstOrDefault(d => !d.IsValid).InternalNr);
		}


		[Test]
		public void WhenMiniDumpsSmartMeterE_GItShouldParseTheGasInDSMRFormatTest() 
		{
			const string source = @"FAFB070DABB7440780/KFM5KAIFA-METER 1-3:0.2.8(40) 0-0:1.0.0(000102045905W) 
				0-0:96.1.1(4530303033303030303030303032343133) 1-0:1.8.1(000001.011*kWh) 
				1-0:1.8.2(000000.000*kWh) 1-0:2.8.1(000000.000*kWh) 
				1-0:2.8.2(000000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(00.034*kW) 
				1-0:2.7.0(00.000*kW) 0-0:17.0.0(999.9*kW) 0-0:96.3.10(1) 0-0:96.7.21(00073) 
				0-0:96.7.9(00020) 1-0:99.97.0(3)(0-0:96.7.19)(000124235657W)(0000003149*s)(000124225935W)(0000000289*s)(000101000001W)(2147483647*s) 
				1-0:32.32.0(00005) 1-0:52.32.0(00006) 1-0:72.32.0(00001) 1-0:32.36.0(00000) 
				1-0:52.36.0(00000) 1-0:72.36.0(00000) 0-0:96.13.1() 0-0:96.13.0() 1-0:31.7.0(000*A) 
				1-0:51.7.0(000*A) 1-0:71.7.0(000*A) 1-0:21.7.0(00.034*kW) 1-0:22.7.0(00.000*kW) 1-0:41.7.0(00.000*kW) 
				1-0:42.7.0(00.000*kW) 1-0:61.7.0(00.000*kW) 1-0:62.7.0(00.000*kW) 0-1:24.1.0(003) 
				0-1:96.1.0(4730303131303033303832373133363133) 0-1:24.2.1(000102043601W)(62869.839*m3) 0-1:24.4.0(1) !583C";


			// Act
			var actual = new MiniR21().Parse(source) as MiniParseResult;

			// Assert
			Assert.IsNotNull(actual);
			Assert.IsInstanceOf<MiniParseModel>(actual.Model);
			Assert.AreEqual(5, actual.Model.Payloads.Count);
			var counterPayload = actual.Model.Payloads[4] as CounterPayload;
			if (counterPayload != null)
				Assert.AreEqual((ulong)62869839, counterPayload.Value);
		}
    }
}
