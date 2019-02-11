using System.Linq;
using NUnit.Framework;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Model.Classes
{
    public class ConfiguredMiniRetrieverTest
    {
        [TestCase("00-00-000-000", QboxType.Duo, "00-00-000-000_00000001_Client0_secondary", DeviceMeterType.NO_Meter)]
        [TestCase("00-00-000-001", QboxType.Mono, "00-00-000-001_00000001_secondary", DeviceMeterType.Smart_Meter_EG)]
        public void TestRetrieve(string qboxSerial, QboxType qboxType, string expectedStorageId, DeviceMeterType expectedMeterType)
        {
            // Arrange
            var retriever = new ConfiguredMiniRetriever(qboxType);

            // Act
            Mini mini = retriever.Retrieve(qboxSerial);

            // Assert
            Counter generationCounter = mini.Counters.Single((c) => c.CounterId == 1);
            Assert.That(generationCounter.StorageId, Is.EqualTo(expectedStorageId));
            Assert.That(mini.MeterType, Is.EqualTo(expectedMeterType));
        }
    }
}
