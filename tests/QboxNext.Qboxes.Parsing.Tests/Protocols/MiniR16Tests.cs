using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters.Validators;
using Xunit;

namespace QboxNext.Qboxes.Parsing.Tests.Protocols
{
    public class MiniR16Tests
    {
        private readonly Mock<ILogger<MiniR16>> _miniLoggerMock = new Mock<ILogger<MiniR16>>();
        private readonly Mock<ILogger<ProtocolReader>> _protocolReaderLoggerMock = new Mock<ILogger<ProtocolReader>>();
        private readonly Mock<ILogger<SmartMeterCounterParser>> _smartMeterLoggerMock = new Mock<ILogger<SmartMeterCounterParser>>();

        private readonly ICounterValueValidator[] _validators =
        {
            new EnergyCounterValueValidator(),
            new GasCounterValueValidator(),
            new LiveCounterValueValidator(),
        };

        //[Theory]
        //[InlineData("2FC30500799E600001F101000178E3", MiniState.InvalidImage)]
        //public void Parse(string message, MiniState miniState)
        //{
        //    // Arrange
        //    var protocolReader = new ProtocolReader(_protocolReaderLoggerMock.Object, message.AsMemory());

        //    var protocolReaderFactoryMock = new Mock<IProtocolReaderFactory>();
        //    protocolReaderFactoryMock.Setup(pr => pr.Create(It.IsAny<ReadOnlyMemory<char>>())).Returns(protocolReader);

        //    var smartMeterCounterParser = new SmartMeterCounterParser(_smartMeterLoggerMock.Object, _validators);
        //    var sut = new MiniR16(_miniLoggerMock.Object, protocolReaderFactoryMock.Object, smartMeterCounterParser);

        //    // Act
        //    var result = (MiniParseResult) sut.Parse(message);

        //    // Assert
        //    result.Model.Status.Status.Should().Be(miniState);
        //}
    }
}