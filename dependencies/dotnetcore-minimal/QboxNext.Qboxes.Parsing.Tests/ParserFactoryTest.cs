using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Factories;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing
{
    [TestFixture]
    public class ParserFactoryTest
    {
        [Test]
        public void WhenAParserIsRegisteredIsShouldBeReturnedByNameTest()
        {
            // Arrange
            ParserFactory.Register(typeof(MiniR07), 2);

            // Act
            ParserFactory.Register(typeof(MiniR16), 1);

            // Assert
            var result = ParserFactory.GetParser("MiniR16") as MiniR16;
            Assert.IsNotNull(result);
        }

        [Test]
        public void ParserFactoryShouldReturnR07Test()
        {
            ParserFactory.RegisterAllParsers();
            // Protocol nr for R16 starts with nr 2
            var result = ParserFactory.GetParserFromMessage("18101010") as MiniR07;

            Assert.IsNotNull(result);
        }

        [Test]
        public void ParserFactoryShouldReturnR16Test()
        {
            ParserFactory.RegisterAllParsers();
            // Protocol nr for R16 starts with nr 39 (0x27)
            var result = ParserFactory.GetParserFromMessage("27101010") as MiniR16;

            Assert.IsNotNull(result);
        }
    }
}