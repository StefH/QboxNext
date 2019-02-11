using System.IO;
using NUnit.Framework;
using QboxNext.MergeQbx.Utils;

namespace QboxNext.MergeQbx.Tests
{
    public class QbxPathUtilsTest
    {
        [Test]
        public void TestGetSerialFromPathWithCorrectPath()
        {
            string serial = QbxPathUtils.GetSerialFromPath(@"./QboxNextData/Qbox_15-46-002-442/15-46-002-442_00000001.qbx");

            Assert.That(serial, Is.EqualTo("15-46-002-442"));
        }

        [Test]
        public void TestGetSerialFromPathWithIncorrectPath()
        {
            Assert.Throws<InvalidDataException>(
                () => QbxPathUtils.GetSerialFromPath(@"./QboxNextData/Qbox_1546-002-442/1546-002-442_00000001.qbx"));
        }

        [Test]
        public void TestGetBaseDirFromPath()
        {
            string baseDir = QbxPathUtils.GetBaseDirFromPath(@"./QboxNextData/Qbox_1546-002-442/1546-002-442_00000001.qbx");

            Assert.That(baseDir, Is.EqualTo($".{Path.DirectorySeparatorChar}QboxNextData"));
        }

        [TestCase(@"./QboxNextData/Qbox_15-46-002-442/15-46-002-442_00000181.qbx", 181)]
        [TestCase(@"./QboxNextData/Qbox_15-46-002-442/15-46-002-442_00000181_Client0.qbx", 181)]
        [TestCase(@"./QboxNextData/Qbox_15-46-002-442/15-46-002-442_00000001_Client0_secondary.qbx", 1)]
        public void TestGetCounterIdFromPath(string path, int expectedCounterId)
        {
            int counterId = QbxPathUtils.GetCounterIdFromPath(path);

            Assert.That(counterId, Is.EqualTo(expectedCounterId));
        }

        [TestCase(@"./QboxNextData/Qbox_15-46-002-442/15-46-002-442_00000181.qbx", null)]
        [TestCase(@"./QboxNextData/Qbox_15-46-002-442/15-46-002-442_00000181_Client0.qbx", "15-46-002-442_00000181_Client0")]
        [TestCase(@"./QboxNextData/Qbox_15-46-002-442/15-46-002-442_00000001_Client0_secondary.qbx", "15-46-002-442_00000001_Client0_secondary")]
        public void TestGetStorageIdFromPath(string path, string expectedStorageId)
        {
            string storageId = QbxPathUtils.GetStorageIdFromPath(path);

            Assert.That(storageId, Is.EqualTo(expectedStorageId));
        }
    }
}
