using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Extensions;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Mini.R07
{
    [TestFixture]
    public class SmartMeterPayLoadTest
    {
        private MiniR07 _sut;

        [SetUp]
        public void Init()
        {
            ServiceProvider services = new ServiceCollection()
                .AddLogging()
                .AddParsers()
                .BuildServiceProvider();

            // Setup static logger factory
            QboxNextLogProvider.LoggerFactory = services.GetRequiredService<ILoggerFactory>();

            _sut = services.GetRequiredService<MiniR07>();
        }

        [Test]
        public void ConstructSmartMeterPayloadTest()
        {
            // Arrange
            const string source = "01C6070A90AE200680/KMP5 KA6U001661160612 0-0:96.1.1(204B413655303031363631313630363132) 1-0:1.8.1(00118.701*kWh) 1-0:1.8.2(00010.000*kWh) 1-0:2.8.1(00200.100*kWh) 1-0:2.8.2(00000.050*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(0000.11*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(999*A) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() !";
            
            // Act
            var actual = _sut.Parse(source);

            // Assert
            Assert.IsNotNull(actual);
            var model = (actual as MiniParseResult).Model as MiniParseModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(4, model.Payloads.Count);
            var counters = model.Payloads.Cast<CounterPayload>().ToList();
            Assert.AreEqual((ulong)118701, counters[0].Value);
            Assert.AreEqual((ulong)10000, counters[1].Value);
            Assert.AreEqual((ulong)200100, counters[2].Value);
            Assert.AreEqual((ulong)50, counters[3].Value);            
        }


		[Test]
		public void CombineAllGAsChannelsTest()
		{
			const string source = @"2863070CB64D680780/ISk5\2MT382-1003 0-0:96.1.1(5A424556303035303838393638333132) 1-0:1.8.1(05381.314*kWh) 1-0:1.8.2(07441.983*kWh) 1-0:2.8.1(00000.000*kWh) 1-0:2.8.2(00000.000*kWh) 0-0:96.14.0(0002) 1-0:1.7.0(0001.13*kW) 1-0:2.7.0(0000.00*kW) 0-0:17.0.0(0999.00*kW) 0-0:96.3.10(1) 0-0:96.13.1() 0-0:96.13.0() 0-1:24.1.0(3) 0-1:96.1.0(3338303034303031313434313233333131) 0-1:24.3.0(131004100000)(00)(60)(1)(0-1:24.2.1)(m3) (04933.286) 0-1:24.4.0(1) 0-2:24.1.0(3) 0-2:96.1.0(3338303034303031333336333236353133) 0-2:24.3.0(131004100000)(00)(60)(1)(0-2:24.2.1)(m3) (00007.511) 0-2:24.4.0(1) !";

            var parseResult = _sut.Parse(source);
            Assert.IsNotNull(parseResult);
            var parseModel = ((MiniParseResult)parseResult).Model;
            var counters = parseModel.Payloads.Cast<CounterPayload>().ToList();
			Assert.AreEqual(4933286 + 7511, counters[4].Value);
		}
    }
}
