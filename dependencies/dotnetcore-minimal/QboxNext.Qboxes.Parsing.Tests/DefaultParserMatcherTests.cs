using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace QboxNext.Qboxes.Parsing
{
    [TestFixture]
    public class DefaultParserMatcherTests
    {
        private readonly ILogger<DefaultParserMatcher> _logger = new NullLogger<DefaultParserMatcher>();
        private const string ValidMessage = "FF-dont-care";
        private List<ParserInfo> _parserInfo;
        private DefaultParserMatcher _sut;

        [SetUp]
        public void SetUp()
        {
            _parserInfo = new List<ParserInfo>
            {
                new ParserInfo
                {
                    MaxProtocolVersion = 2
                },
                new ParserInfo
                {
                    MaxProtocolVersion = 4
                },
                new ParserInfo
                {
                    MaxProtocolVersion = 17
                }
            };

            _sut = new DefaultParserMatcher(_parserInfo, _logger);
        }

        [Test]
        public void When_a_single_parser_is_registered_it_should_always_return_that()
        {
            _sut = new DefaultParserMatcher(new[]
            {
                _parserInfo[0]
            }, _logger);

            // Act
            ParserInfo actual = _sut.Match(ValidMessage);

            // Assert
            actual.Should().Be(_parserInfo[0]);
        }

        [Test]
        public void When_no_parsers_are_registered_it_should_throw()
        {
            // Act
            Action act = () => new DefaultParserMatcher(new ParserInfo[0], _logger);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .Which.ParamName
                .Should()
                .Be("registeredParsers");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("1")]
        public void When_message_is_too_short_it_should_return_null(string message)
        {
            // Act
            ParserInfo actual = _sut.Match(message);

            // Assert
            actual.Should().BeNull();
        }

        [TestCase("Z0")]
        [TestCase("I(")]
        public void When_message_does_not_start_with_2_hex_bytes_it_should_return_null(string message)
        {
            // Act
            ParserInfo actual = _sut.Match(message);

            // Assert
            actual.Should().BeNull();
        }

        [TestCase("01-protocol-message", 2, Description = "No parser matches, so it returns first. Is this correct?")]
        [TestCase("02-protocol-message", 2)]
        [TestCase("03-protocol-message", 2)]
        [TestCase("04-protocol-message", 4)]
        [TestCase("05-protocol-message", 4)]
        [TestCase("0F-protocol-message", 4)]
        [TestCase("10-protocol-message", 4)]
        [TestCase("11-protocol-message", 17)]
        [TestCase("12-protocol-message", 17)]
        public void When_message_matches_should_return_best_matching_parserInfo(string message, int expectedProtocolVersion)
        {
            // Act
            ParserInfo actual = _sut.Match(message);

            // Assert
            actual.Should().NotBeNull();
            actual.MaxProtocolVersion.Should().Be(expectedProtocolVersion);
        }
    }
}
