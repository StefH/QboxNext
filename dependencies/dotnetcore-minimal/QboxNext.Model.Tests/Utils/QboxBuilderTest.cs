using System.Linq;
using NUnit.Framework;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Model.Utils
{
    public class QboxBuilderTest
    {
        private QboxBuilder _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new QboxBuilder();
        }

        [Test]
        public void TestEmpty()
        {
            Assert.Throws(typeof(QboxBuilderException), () => _sut.Build());
        }

        [Test]
        public void TestNoSerial()
        {
            // Arrange
            _sut.WithType(QboxType.Mono)
                .WithCounter(181, CounterType.Primary, Core.Dto.DeviceEnergyType.NetLow);

            // Act & Assert
            Assert.Throws(typeof(QboxBuilderException), () => _sut.Build());
        }

        [Test]
        public void TestNoCounters()
        {
            // Arrange
            _sut.WithType(QboxType.Mono)
                .WithSerial("00-00-000-000");

            // Act & Assert
            Assert.Throws(typeof(QboxBuilderException), () => _sut.Build());
        }

        [Test]
        public void TestInvalidCounter()
        {
            // Arrange
            _sut.WithType(QboxType.Mono)
                .WithSerial("00-00-000-000")
                .WithCounter(42, CounterType.Primary, Core.Dto.DeviceEnergyType.Gas);

            // Act & Assert
            Assert.Throws(typeof(QboxBuilderException), () => _sut.Build());
        }

        [Test]
        public void TestValidMono()
        {
            // Arrange
            _sut.WithType(QboxType.Mono)
                .WithSerial("00-00-000-000")
                .WithCounter(181, CounterType.Primary, Core.Dto.DeviceEnergyType.NetLow)
                .WithCounter(182, CounterType.Primary, Core.Dto.DeviceEnergyType.NetHigh);

            // Act
            Mini mini = _sut.Build();

            // Assert
            Assert.That(mini.SerialNumber, Is.EqualTo("00-00-000-000"));
            Assert.That(mini.MeterType, Is.EqualTo(DeviceMeterType.Smart_Meter_EG));
            Assert.That(mini.SecondaryMeterType, Is.EqualTo(DeviceMeterType.SO_Pulse));
            Assert.That(mini.Counters.Count, Is.EqualTo(2));
            Assert.That(mini.Clients.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestValidDuo()
        {
            // Arrange
            _sut.WithType(QboxType.Duo)
                .WithSerial("00-00-000-001")
                .WithCounter(181, CounterType.Primary, Core.Dto.DeviceEnergyType.NetLow)
                .WithCounter(182, CounterType.Primary, Core.Dto.DeviceEnergyType.NetHigh)
                .WithCounter(281, CounterType.Primary, Core.Dto.DeviceEnergyType.NetLow)
                .WithCounter(282, CounterType.Primary, Core.Dto.DeviceEnergyType.NetHigh);

            // Act
            Mini mini = _sut.Build();

            // Assert
            Assert.That(mini.SerialNumber, Is.EqualTo("00-00-000-001"));
            Assert.That(mini.MeterType, Is.EqualTo(DeviceMeterType.NO_Meter));
            Assert.That(mini.SecondaryMeterType, Is.EqualTo(DeviceMeterType.NO_Meter));
            Assert.That(mini.Counters.Count, Is.EqualTo(4));
            Assert.That(mini.Clients.Count, Is.EqualTo(1));
            Assert.That(mini.Clients.First().MeterType, Is.EqualTo(DeviceMeterType.Smart_Meter_EG));
            Assert.That(mini.Clients.First().SecondaryMeterType, Is.EqualTo(DeviceMeterType.SO_Pulse));
        }

        [TestCase(1, CounterType.Primary, false)]
        [TestCase(1, CounterType.Secondary, true)]
        public void TestSecondary(int counterId, CounterType counterType, bool expectedSecondary)
        {
            // Arrange
            _sut.WithType(QboxType.Mono)
                .WithSerial("00-00-000-000")
                .WithCounter(counterId, counterType, Core.Dto.DeviceEnergyType.NetLow);

            // Act
            Mini mini = _sut.Build();

            // Assert
            Assert.That(mini.Counters.First().Secondary, Is.EqualTo(expectedSecondary));
        }
    }
}
