using System;
using System.Diagnostics;
using System.Xml;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Protocol
{
    [TestFixture]
    public class ProtocolReaderTests
    {
        private IProtocolReader _parser;

        [TearDown]
        public void TearDown()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            (_parser as IDisposable)?.Dispose();
        }

        protected virtual IProtocolReader CreateSubject(string data)
        {
            return new ProtocolReader(new NullLogger<ProtocolReader>(), data);
        }

        [TestCase("F")]
        [TestCase("A0F", (byte)0xA0)]
        [TestCase("B0F13", (byte)0xB0, (byte)0xF1)]
        [TestCase("341C3E2", (byte)0x34, (byte)0x1C, (byte)0x3E)]
        public void When_reading_non_padded_hexadecimal_string_it_should_throw(string dataToParse, params byte[] expectedData)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);
            int position = 0;

            // Act
            Func<byte> readByte = () => _parser.ReadByte();
            foreach (byte expectedByte in expectedData)
            {
                position += 2;
                readByte.Should()
                    .NotThrow("two or more characters are still on the stream")
                    .Which.Should()
                    .Be(expectedByte);
            }

            // Assert
            _parser.AtEndOfStream()
                .Should()
                .BeFalse("the last char is not read yet");

            ProtocolReaderException ex = readByte
                .Should()
                .Throw<ProtocolReaderException>("the stream does not have enough hex chars left")
                .Which;

            ex.Message.Should().StartWith("Reached end of stream");
            ex.Position.Should().Be(position);
            ex.Length.Should().Be(dataToParse.Length);

            _parser.AtEndOfStream().Should().BeTrue();
        }

        [Test]
        public void When_reading_byte_from_data_that_is_not_hexadecimal_it_should_throw()
        {
            // Arrange 
            _parser = CreateSubject("Zs");

            // Act
            Action readByte = () => _parser.ReadByte();

            // Assert
            ProtocolReaderException ex = readByte.Should()
                .Throw<ProtocolReaderException>()
                .Which;

            ex.InnerException.Should().BeOfType<FormatException>()
                .Which.Message.Should().Be("Could not find any recognizable digits.");
            ex.Position.Should().Be(0);
            ex.Length.Should().Be(2);

            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("", 0)]
        [TestCase("A0", 1)]
        [TestCase("B0F1", 2)]
        [TestCase("341C3E", 3)]
        public void When_reading_beyond_end_of_stream_it_should_throw(string dataToParse, int successfulReadsCount)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);
            if (!string.IsNullOrEmpty(dataToParse))
            {
                _parser.AtEndOfStream().Should().BeFalse();
            }

            // Act
            Func<byte> readByte = () => _parser.ReadByte();

            for (int i = 0; i < successfulReadsCount; i++)
            {
                readByte.Should().NotThrow();
            }

            // Assert
            _parser.AtEndOfStream()
                .Should()
                .BeTrue("all hex data was read");

            readByte
                .Should()
                .Throw<ProtocolReaderException>()
                .Which.Message
                .Should().StartWith("Reached end of stream");
        }

        [TestCase("1234FEDC", (byte)0x12, (byte)0x34, (byte)0xFE, (byte)0xDC)]
        public void When_reading_byte_should_read_correct_value(string dataToParse, params byte[] expectedValues)
        {
            // Arrange
            _parser = CreateSubject(dataToParse);

            // Act
            foreach (byte expectedValue in expectedValues)
            {
                byte actual = _parser.ReadByte();

                // Assert
                actual.Should().Be(expectedValue);
            }

            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("1234FEDC01020304", 0x1234FEDC, 0x01020304)]
        public void When_reading_int_should_read_correct_value(string dataToParse, params int[] expectedValues)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);

            // Act
            foreach (int expectedValue in expectedValues)
            {
                int actual = _parser.ReadInt32();

                // Assert
                actual.Should().Be(expectedValue);
            }

            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("1234FEDCFFFFA0A0", (uint)0x1234FEDC, 0xFFFFA0A0)]
        public void When_reading_uint_should_read_correct_value(string dataToParse, params uint[] expectedValues)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);

            // Act
            foreach (uint expectedValue in expectedValues)
            {
                uint actual = _parser.ReadUInt32();

                // Assert
                actual.Should().Be(expectedValue);
            }

            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("12347ABC", (short)0x1234, (short)0x7ABC)]
        public void When_reading_short_should_read_correct_value(string dataToParse, params short[] expectedValues)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);

            // Act
            foreach (short expectedValue in expectedValues)
            {
                short actual = _parser.ReadInt16();

                // Assert
                actual.Should().Be(expectedValue);
            }

            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("1234FEDC", (ushort)0x1234, (ushort)0xFEDC)]
        public void When_reading_ushort_should_read_correct_value(string dataToParse, params ushort[] expectedValues)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);

            // Act
            foreach (ushort expectedValue in expectedValues)
            {
                ushort actual = _parser.ReadUInt16();

                // Assert
                actual.Should().Be(expectedValue);
            }

            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("1234FEDC", "2016-09-05T10:50:04Z")]
        [TestCase("12CFF780", "2017-01-01T00:00:00Z")]
        [TestCase("1EF5DD8A", "2023-06-17T21:58:34Z")]
        public void When_reading_datetime_should_read_correct_value(string dataToParse, string expectedDateStr)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);
            DateTime expectedDate = XmlConvert.ToDateTime(expectedDateStr, XmlDateTimeSerializationMode.Unspecified);

            // Act
            DateTime actual = _parser.ReadDateTime();

            // Assert
            actual.Should().Be(expectedDate);
            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("AF12RestOfString", (byte)0xAF, (byte)0x12, "RestOfString")]
        public void When_reading_to_end_should_return_correct_string(string dataToParse, params object[] expectedValues)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);

            // Act & Assert
            _parser.ReadByte().Should().Be((byte)expectedValues[0]);
            _parser.ReadByte().Should().Be((byte)expectedValues[1]);
            _parser.ReadToEnd().Should().Be((string)expectedValues[2]);
            _parser.AtEndOfStream().Should().BeTrue();
        }

        [TestCase("|Some_Data||More-data|", '|', "Some_Data", "More-data")]
        [TestCase("||||", '|', "", "")]
        [TestCase("|No-trailing", '|', "No-trailing")]
        public void When_reading_encapsulated_text_should_return_correct_data(string dataToParse, char delimiter, params string[] expectedValues)
        {
            // Arrange 
            _parser = CreateSubject(dataToParse);

            // Act
            foreach (string expectedValue in expectedValues)
            {
                string actual = _parser.ReadEncapsulatedString(delimiter);

                actual.Should().Be(expectedValue);
            }

            // Assert
            _parser.AtEndOfStream().Should().BeTrue();
        }

        [Test]
        public void When_reading_encapsulated_text_that_does_not_start_with_char_should_throw()
        {
            // Arrange 
            _parser = CreateSubject("data");

            // Act
            Action act = () => _parser.ReadEncapsulatedString('|');

            // Assert
            ProtocolReaderException ex = act.Should().Throw<ProtocolReaderException>().Which;
            ex.Position.Should().Be(0);
            ex.Length.Should().Be(4);
        }
    }
}