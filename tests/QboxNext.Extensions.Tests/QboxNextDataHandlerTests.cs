using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using QboxNext.Core.Interfaces;
using QboxNext.Extensions.Implementations;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Logging;
using QboxNext.Model.Classes;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators;
using Xunit;

namespace QboxNext.Extensions.Tests
{
    public class QboxNextDataHandlerTests
    {
        private readonly Mock<ILogger<QboxNextDataHandler>> _qboxNextDataHandlerLoggerMock = new Mock<ILogger<QboxNextDataHandler>>();
        private readonly Mock<ILogger<MiniR21>> _miniLoggerMock = new Mock<ILogger<MiniR21>>();
        private readonly Mock<ILogger<ProtocolReader>> _protocolReaderLoggerMock = new Mock<ILogger<ProtocolReader>>();
        private readonly Mock<ILogger<SmartMeterCounterParser>> _smartMeterLoggerMock = new Mock<ILogger<SmartMeterCounterParser>>();

        private readonly Mock<ICounterStoreService> _counterStoreServiceMock = new Mock<ICounterStoreService>();
        private readonly Mock<IStateStoreService> _stateStoreServiceMock = new Mock<IStateStoreService>();

        private readonly Mock<IDateTimeService> _dateMock;

        private readonly ICounterValueValidator[] _validators =
        {
            new EnergyCounterValueValidator(),
            new GasCounterValueValidator(),
            new LiveCounterValueValidator(),
        };

        public QboxNextDataHandlerTests()
        {
            QboxNextLogProvider.LoggerFactory = new LoggerFactory();

            _dateMock = new Mock<IDateTimeService>();
            _dateMock.SetupGet(d => d.UtcNow).Returns(new DateTime(2020, 3, 16).ToUniversalTime);
            _dateMock.SetupGet(d => d.Now).Returns(new DateTime(2020, 3, 16));
        }

        [Theory]
        [InlineData("2F940718D6D5A41E00", "9418D6728000")]
        [InlineData("2F610718D705B01E00", "6118D6728000")]
        [InlineData("2F9B0718D713481E00", "9B18D6728000")]
        [InlineData("2F100718D9A3C41E00", "1018D6728000")] // Qbox van R. versie 47 nieuw
        public async Task Handle_NoMeterMessage(string message, string response)
        {
            string correlationId = Guid.NewGuid().ToString();

            var mini = new Mini();
            var ctx = new QboxDataDumpContext(message, 16, "localhost", "::1", mini);

            var parserFactoryMock = new Mock<IParserFactory>();
            var protocolReader = new ProtocolReader(_protocolReaderLoggerMock.Object, message.AsMemory());

            var protocolReaderFactoryMock = new Mock<IProtocolReaderFactory>();
            protocolReaderFactoryMock.Setup(pr => pr.Create(It.IsAny<ReadOnlyMemory<char>>())).Returns(protocolReader);

            var smartMeterCounterParser = new SmartMeterCounterParser(_smartMeterLoggerMock.Object, _validators);
            parserFactoryMock.Setup(pf => pf.GetParser(It.IsAny<string>())).Returns(new MiniR21(_miniLoggerMock.Object, protocolReaderFactoryMock.Object, smartMeterCounterParser));

            var sut = new QboxNextDataHandler(correlationId, ctx, parserFactoryMock.Object, _counterStoreServiceMock.Object, _stateStoreServiceMock.Object, _dateMock.Object, _qboxNextDataHandlerLoggerMock.Object);

            var result = await sut.HandleAsync();
            var resultAsText = result.Trim('\x02', '\x03');

            resultAsText.Should().Be(response);
        }

        [Fact]
        public async Task Handle_DataMessage()
        {
            string correlationId = Guid.NewGuid().ToString();
            const string message =
@"FAFB070DABB7440780/KFM5KAIFA-METER 1-3:0.2.8(40) 0-0:1.0.0(000102045905W) 
0 - 0:96.1.1(4530303033303030303030303032343133)
1 - 0:1.8.1(000181.011 * kWh)
1 - 0:1.8.2(000182.044 * kWh)
1 - 0:2.8.1(000281.099 * kWh)
1 - 0:2.8.2(000282.077 * kWh)
0 - 0:96.14.0(0001) 1 - 0:1.7.0(00.034 * kW)
1 - 0:2.7.0(00.000 * kW) 0 - 0:17.0.0(999.9 * kW) 0 - 0:96.3.10(1) 0 - 0:96.7.21(00073)
0 - 0:96.7.9(00020) 1 - 0:99.97.0(3)(0 - 0:96.7.19)(000124235657W)(0000003149 * s)(000124225935W)(0000000289 * s)(000101000001W)(2147483647 * s)
1 - 0:32.32.0(00005) 1 - 0:52.32.0(00006) 1 - 0:72.32.0(00001) 1 - 0:32.36.0(00000)
1 - 0:52.36.0(00000) 1 - 0:72.36.0(00000) 0 - 0:96.13.1() 0 - 0:96.13.0() 1 - 0:31.7.0(000 * A)
1 - 0:51.7.0(000 * A) 1 - 0:71.7.0(000 * A) 1 - 0:21.7.0(00.034 * kW) 1 - 0:22.7.0(00.000 * kW) 1 - 0:41.7.0(00.000 * kW)
1 - 0:42.7.0(00.000 * kW) 1 - 0:61.7.0(00.000 * kW) 1 - 0:62.7.0(00.000 * kW) 0 - 1:24.1.0(003)
0 - 1:96.1.0(4730303131303033303832373133363133)
0 - 1:24.2.1(000102043601W)(72869.839 * m3) 0 - 1:24.4.0(1)!583C";

            var mini = new Mini();
            var ctx = new QboxDataDumpContext(message, 16, "localhost", "::1", mini);

            var parserFactoryMock = new Mock<IParserFactory>();
            var protocolReader = new ProtocolReader(_protocolReaderLoggerMock.Object, message.AsMemory());

            var protocolReaderFactoryMock = new Mock<IProtocolReaderFactory>();
            protocolReaderFactoryMock.Setup(pr => pr.Create(It.IsAny<ReadOnlyMemory<char>>())).Returns(protocolReader);

            var smartMeterCounterParser = new SmartMeterCounterParser(_smartMeterLoggerMock.Object, _validators);
            parserFactoryMock.Setup(pf => pf.GetParser(It.IsAny<string>())).Returns(new MiniR21(_miniLoggerMock.Object, protocolReaderFactoryMock.Object, smartMeterCounterParser));

            var sut = new QboxNextDataHandler(correlationId, ctx, parserFactoryMock.Object, _counterStoreServiceMock.Object, _stateStoreServiceMock.Object, _dateMock.Object, _qboxNextDataHandlerLoggerMock.Object);

            var result = await sut.HandleAsync();
            var resultAsText = result.Trim('\x02', '\x03');

            resultAsText.Should().Be("FB18D6728000");
        }
    }
}