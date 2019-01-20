using System.Linq;
using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Mini.R16
{
    [TestFixture]
    public class MiniR16Test
    {
        [Test]
        public void MiniR16ParseShouldContainGroupIDTest()
        {
            // Arrange
            const string source = "2726070BB823281E0181010000010D";
            //                     1 2 3 4       5 6 7 8
            // 1 Protocol nr
            // 2 Sequence nr
            // 3 Device status
            // 4 Time
            // 5 Primary metertype
            // 6 Payload indicator
            // 7 Counter payload group id

            // Act
            var actual = new MiniR16().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);            
        }

        
        [Test]
        public void MiniR16ParseShouldContainGroupIDAndThreeCounterPayloadsTest()
        {
            // Arrange
            const string source = "27F8070BB831B01E0382010000026D030000000101010000005E";
            //                     1 2 3 4       5 6 7g  --------  --------gg  --------
            // 1 Protocol nr
            // 2 Sequence nr
            // 3 Device status
            // 4 Time
            // 5 Primary metertype
            // 6 Payload indicator
            // 7 Counter payload group id  (g = group, - = counter value)

            // Act
            var actual = new MiniR16().Parse(source) as MiniParseResult;

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Model.Payloads.Count() == 3);
        }


        [Test]
        public void MiniR16ParseShouldHaveP1DataTest()
        {
            // Arrange
            const string source = @"2703070BB7DE041E5601054400""/ISk5\2MT382-1003
                0-0:96.1.1(5A424556303035303933373935353132)
                1-0:1.8.1(00067.368*kWh)
                1-0:1.8.2(00056.937*kWh)
                1-0:2.8.1(00000.000*kWh)
                1-0:2.8.2(00000.000*kWh)
                0-0:96.14.0(0002)
                1-0:1.7.0(0000.89*kW)
                1-0:2.7.0(0000.00*kW)
                0-0:17.0.0(0999.00*kW)
                0-0:96.3.10(1)
                0-0:96.13.1()
                0-0:96.13.0()
                !
                ""8601000107280300000000020000DE7604000000000500000000060000037A
                ";

            // Act
            var actual = new MiniR16().Parse(source) as MiniParseResult;

            var deviceSettings = actual.Model.Payloads.Single(s => s.GetType() == typeof(DeviceSettingsPayload)) as DeviceSettingsPayload;
            Assert.IsNotNull(deviceSettings);
            Assert.AreEqual(deviceSettings.DeviceSetting, DeviceSettingType.ClientRawP1Data);

            var counters = actual.Model.Payloads.Where(s => s.GetType().IsSubclassOf(typeof(CounterPayload))).Cast<CounterPayload>();
            Assert.AreEqual(counters.FirstOrDefault(d => d.InternalNr == 1).Value, 67368);  // 1 = 1.8.1 (Consumed energy tariff I)
            Assert.AreEqual(counters.FirstOrDefault(d => d.InternalNr == 2).Value, 56950);  // 1 = 1.8.2 (Consumed energy tariff II)
            Assert.AreEqual(counters.FirstOrDefault(d => d.InternalNr == 3).Value, 0);  // 1 = 2.8.1 (Produced energy tariff I)
            Assert.AreEqual(counters.FirstOrDefault(d => d.InternalNr == 4).Value, 0);  // 1 = 2.8.2 (Produced energy tariff II)
            Assert.AreEqual(counters.FirstOrDefault(d => d.InternalNr == 5).Value, 0);  // 1 = 2.7.0 (Actual produced power)
            Assert.AreEqual(counters.FirstOrDefault(d => d.InternalNr == 6).Value, 890);  // 1 = 1.7.0 (Actual consumed power)
        }


		[Test]
		public void MiniR16ParseDsmr5Test()
		{
			// Arrange
			const string source = @"2F8B07137939940681710100006A61/XMX5LGBBLB2410012572
    
    1-3:0.2.8(50)
    0-0:1.0.0(170404234727S)
    0-0:96.1.1(4530303336303033373535393838343136)
    1-0:1.8.1(000005.285*kWh)
    1-0:1.8.2(000013.104*kWh)
    1-0:2.8.1(000000.000*kWh)
    1-0:2.8.2(000022.875*kWh)
    0-0:96.14.0(0001)
    1-0:1.7.0(01.016*kW)
    1-0:2.7.0(00.000*kW)
    0-0:96.7.21(00010)
    0-0:96.7.9(00000)
    1-0:99.97.0(0)(0-0:96.7.19)
    1-0:32.32.0(00000)
    1-0:52.32.0(00000)
    1-0:72.32.0(00000)
    1-0:32.36.0(00000)
    1-0:52.36.0(00000)
    1-0:72.36.0(00000)
    0-0:96.13.0()
    1-0:32.7.0(0226.0*V)
    1-0:52.7.0(0227.0*V)
    1-0:72.7.0(0231.0*V)
    1-0:31.7.0(2.08*A)
    1-0:51.7.0(0.48*A)
    1-0:71.7.0(2.25*A)
    1-0:21.7.0(00.420*kW)
    1-0:41.7.0(00.081*kW)
    1-0:61.7.0(00.514*kW)
    1-0:22.7.0(00.000*kW)
    1-0:42.7.0(00.000*kW)
    1-0:62.7.0(00.000*kW)
    !9C66";

			// Act
			var actual = new MiniR16().Parse(source) as MiniParseResult;

			var counters = actual.Model.Payloads.Cast<CounterPayload>().ToList();
			Assert.AreEqual(27233, counters.FirstOrDefault(d => d.InternalNr == 1).Value);  // S0
			Assert.AreEqual(5285, counters.FirstOrDefault(d => d.InternalNr == 181).Value);
			Assert.AreEqual(13104, counters.FirstOrDefault(d => d.InternalNr == 182).Value);
			Assert.AreEqual(0, counters.FirstOrDefault(d => d.InternalNr == 281).Value);
			Assert.AreEqual(22875, counters.FirstOrDefault(d => d.InternalNr == 282).Value);
		}
	}

}
