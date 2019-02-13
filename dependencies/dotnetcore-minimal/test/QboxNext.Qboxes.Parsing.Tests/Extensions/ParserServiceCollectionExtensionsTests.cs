using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Extensions
{
    [TestFixture]
    public class ParserServiceCollectionExtensionsTests
    {
        [TestCase(typeof(IParserFactory))]
        [TestCase(typeof(IParserMatcher))]
        [TestCase(typeof(MiniR07))]
        [TestCase(typeof(MiniR16))]
        [TestCase(typeof(MiniR21))]
        [TestCase(typeof(MiniResponse))]
        [TestCase(typeof(IEnumerable<ParserInfo>))]
        public void When_adding_parsers_should_register_service(Type serviceType)
        {
            // Act
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddParsers()
                .BuildServiceProvider();

            // Assert
            serviceProvider.GetService(serviceType).Should().NotBeNull();
        }
    }
}
