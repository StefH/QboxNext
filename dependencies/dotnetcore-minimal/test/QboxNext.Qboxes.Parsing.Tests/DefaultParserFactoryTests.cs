using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Extensions;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing
{
    [TestFixture]
    public class DefaultParserFactoryTests
    {
        private Mock<IMessageParser> _dummyParserMock;
        private Mock<IParserMatcher> _parserMatcherMock;
        private IServiceProvider _serviceProvider;
        private DefaultParserFactory _sut;

        [SetUp]
        public void SetUp()
        {
            _dummyParserMock = new Mock<IMessageParser>();

            _serviceProvider = new ServiceCollection()
                .AddSingleton(_dummyParserMock.Object.GetType(), _dummyParserMock.Object)
                .BuildServiceProvider();

            _parserMatcherMock = new Mock<IParserMatcher>();
            _parserMatcherMock
                .Setup(m => m.Match(It.IsAny<string>()))
                .Returns(new ParserInfo
                {
                    Type = _dummyParserMock.Object.GetType()
                });

            _parserMatcherMock
                .Setup(m => m.Match("message-with-no-matching-parser"))
                .Returns((ParserInfo)null);

            _sut = new DefaultParserFactory(_serviceProvider, _parserMatcherMock.Object);
        }

        [Test]
        public void When_parser_matcher_finds_match_it_should_return_a_message_parser()
        {
            // Act
            IMessageParser actual = _sut.GetParser("message-data");

            // Assert
            actual.Should().Be(_dummyParserMock.Object);
        }

        [Test]
        public void When_parser_matcher_finds_no_match_it_should_throw()
        {
            // Act
            Action act = () => _sut.GetParser("message-with-no-matching-parser");

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestCase("18101010", typeof(MiniR07), Description = "Protocol nr for R16 starts with nr 2")]
        [TestCase("27101010", typeof(MiniR16), Description = "Protocol nr for R16 starts with nr 39 (0x27)")]
        public void When_using_e2e_parser_configuration_should_return_correct_parser(string message, Type expectedParserType)
        {
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddParsers()
                .BuildServiceProvider();
            _sut = (DefaultParserFactory)serviceProvider.GetRequiredService<IParserFactory>();

            // Act
            IMessageParser actual = _sut.GetParser(message);

            // Assert
            actual.Should().BeOfType(expectedParserType);

        }
    }
}
