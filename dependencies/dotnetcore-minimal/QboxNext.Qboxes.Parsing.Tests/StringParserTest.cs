using System;
using NUnit.Framework;

namespace QboxNext.Qboxes.Parsing
{
    [TestFixture]
    public class StringParserTest
    {
        [Test]
        public void StringParserShouldReturnByteTest()
        {
            // Arrange
            var parser = new StringParser("01");

            // Act
            var actual = parser.ParseByte();

            // Assert
            Assert.AreEqual(1, actual);
        }

        
		[Test]
        public void StringParserShouldReturn255Test()
        {
            //Arrange 
            var parser = new StringParser("FF");

            // Act
            var actual = parser.ParseByte();

            // assert
            Assert.AreEqual(255, actual);
        }

        
		[Test]
        public void StringParserShouldReturnExceptionTest()
        {
            //Arrange 
            var parser = new StringParser("F");

            // Act
            try
            {
                var actual = parser.ParseByte();
                // assert
                Assert.Fail("Exception expected!");
            }
            catch (OverflowException)
            {
                // exception expected!
            }
        }

        
		[Test]
        public void StringParserShouldReturnExceptionWhenHexStringIncorrectTest()
        {
            //Arrange 
            var parser = new StringParser("Zs");

            // Act
            try
            {
                var actual = parser.ParseByte();
                // assert
                Assert.Fail("Exception expected!");
            }
            catch (FormatException)
            {
                // exception expected!
            }
        }

        
		[Test]
        public void StringParserShouldReturnByteWhitLongStringTest()
        {
            //Arrange 
            var parser = new StringParser("EEThisIsTheRest");

            // Act
            var actual = parser.ParseByte();
            
            // assert
            Assert.AreEqual(238, actual);
        }

        
		[Test]
        public void StringParserShouldReturnWhForStringTest()
        {
            // arrange
            var parser = new StringParser("");
            // assert
            // 7 digits 1 decimals
            Assert.AreEqual((ulong)12294600, parser.ReadSmartMeterCounterValue("(0012294.6*kWh)", 181));
            // 6 digits 2 decimals
            Assert.AreEqual((ulong)1229460, parser.ReadSmartMeterCounterValue("(001229.46*kWh)", 181));
            // 5 digits 3 decimals
            Assert.AreEqual((ulong)122946, parser.ReadSmartMeterCounterValue("(00122.946*kWh)", 181));
            // 6 digits 3 decimals
            Assert.AreEqual((ulong)122946, parser.ReadSmartMeterCounterValue("(000122.946*kWh)", 181));

            // 7 digits 1 decimals
            Assert.AreEqual((ulong)12294600, parser.ReadSmartMeterCounterValue("(0012294.6*kWh)", 2421));
            // 6 digits 2 decimals
            Assert.AreEqual((ulong)1229460, parser.ReadSmartMeterCounterValue("(001229.46*kWh)", 2421));
            // 5 digits 3 decimals
            Assert.AreEqual((ulong)122946, parser.ReadSmartMeterCounterValue("(00122.946*kWh)", 2421));
        }

        
		[Test]
        public void StringParserShouldReturnExceptionForInvalidFormatTest()
        {
            // arrange
            var parser = new StringParser("");
            // assert
            // 8 digits 0 decimals
            // qplat-116: waarden zonder decimalen punt niet meer accepteren
            Assert.Throws<InvalidFormatException>(() => parser.ReadSmartMeterCounterValue("(00122946*kWh)", 181));
            // 8 digits 0 decimals
            Assert.Throws<InvalidFormatException>(() => parser.ReadSmartMeterCounterValue("(00122946*kWh)", 2421));
            Assert.Throws<InvalidFormatException>(() => parser.ReadSmartMeterCounterValue("1-0:1.8.1(0012.946*kWh)", 181));
            Assert.Throws<InvalidFormatException>(() => parser.ReadSmartMeterCounterValue("1-0:1.8.1(0000012.96*kWh)", 181));
            Assert.Throws<InvalidFormatException>(() => parser.ReadSmartMeterCounterValue("1-0:1.8.1(12946*kWh)", 181));
        }


	    [Test]
	    public void ReadSmartMeterCounterValueForWindParkSmartMeterTest()
	    {
		    // Arrange
			var parser = new StringParser("");

			// Act & Assert
			Assert.AreEqual(1002703872, parser.ReadSmartMeterCounterValue("(1002703.872*kWh)", 281));
	    }
    }
}
