using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Extensions;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Mini.Response
{
    [TestFixture]
    public class MiniResponseTest
    {
        private MiniResponse _sut;

        [SetUp]
        public void SetUp()
        {
            ServiceProvider services = new ServiceCollection()
                .AddLogging()
                .AddParsers()
                .BuildServiceProvider();

            // Setup static logger factory
            QboxNextLogProvider.LoggerFactory = services.GetRequiredService<ILoggerFactory>();

            _sut = services.GetRequiredService<MiniResponse>();
        }

        [Test]
        public void ParseResponseWithFirmwareUrlTest()
        {
            // Arrange, voorbeeld response uit qplat-52
            const string source = "040A470BD80001\"firmware-acc.QboxNext.nl\"02\"qserver-acc.QboxNext.nl\"";

            // Act
            var actual = _sut.Parse(source) as ResponseParseResult;

            // Assert
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.DeviceSettings);
            Assert.IsTrue(actual.DeviceSettings.Count() == 2, "2 device settings expected");
        }
    }
}
