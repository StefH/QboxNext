using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators;

namespace QboxNext.Qboxes.Parsing.Protocol.SmartMeters
{
    [TestFixture]
    public class SmartMeterCounterParserTests
    {
        [Test]
        public void ShouldReturnWhForStringTest()
        {
            // arrange
            var parser = new SmartMeterCounterParser(new NullLogger<SmartMeterCounterParser>(), new ICounterValueValidator[]
            {
                new EnergyCounterValueValidator(),
                new GasCounterValueValidator()
            });

            // assert
            // 7 digits 1 decimals
            Assert.AreEqual((ulong)12294600, parser.Parse("(0012294.6*kWh)", 181));
            // 6 digits 2 decimals
            Assert.AreEqual((ulong)1229460, parser.Parse("(001229.46*kWh)", 181));
            // 5 digits 3 decimals
            Assert.AreEqual((ulong)122946, parser.Parse("(00122.946*kWh)", 181));
            // 6 digits 3 decimals
            Assert.AreEqual((ulong)122946, parser.Parse("(000122.946*kWh)", 181));

            // 7 digits 1 decimals
            Assert.AreEqual((ulong)12294600, parser.Parse("(0012294.6*kWh)", 2421));
            // 6 digits 2 decimals
            Assert.AreEqual((ulong)1229460, parser.Parse("(001229.46*kWh)", 2421));
            // 5 digits 3 decimals
            Assert.AreEqual((ulong)122946, parser.Parse("(00122.946*kWh)", 2421));
        }

        [Test]
        public void ShouldReturnExceptionForInvalidFormatTest()
        {
            // arrange
            var parser = new SmartMeterCounterParser(new NullLogger<SmartMeterCounterParser>(), new ICounterValueValidator[]
            {
                new EnergyCounterValueValidator(),
                new GasCounterValueValidator(),
                new LiveCounterValueValidator()
            });

            // assert
            // 8 digits 0 decimals
            // qplat-116: waarden zonder decimalen punt niet meer accepteren
            Assert.Throws<SmartMeterProtocolException>(() => parser.Parse("(00122946*kWh)", 181));
            // 8 digits 0 decimals
            Assert.Throws<SmartMeterProtocolException>(() => parser.Parse("(00122946*kWh)", 2421));
            Assert.Throws<SmartMeterProtocolException>(() => parser.Parse("1-0:1.8.1(0012.946*kWh)", 181));
            Assert.Throws<SmartMeterProtocolException>(() => parser.Parse("1-0:1.8.1(0000012.96*kWh)", 181));
            Assert.Throws<SmartMeterProtocolException>(() => parser.Parse("1-0:1.8.1(12946*kWh)", 181));
        }

        [Test]
        public void ReadSmartMeterCounterValueForWindParkSmartMeterTest()
        {
            // Arrange
            var parser = new SmartMeterCounterParser(new NullLogger<SmartMeterCounterParser>(), new ICounterValueValidator[]
            {
                new EnergyCounterValueValidator()
            });

            // Act & Assert
            Assert.AreEqual(1002703872, parser.Parse("(1002703.872*kWh)", 281));
        }
    }
}
