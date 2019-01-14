using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using QboxNext.Qserver.Classes;

namespace QboxNext.Server.Classes
{
    public class FirmwareVersionMapperTest
    {
        private FirmwareVersionMapper _firmwareVersionMapper;

        [SetUp]
        public void SetUp()
        {
            _firmwareVersionMapper = new FirmwareVersionMapper();
        }

        [TestCase("00-00-000-000", "A14_Encrypt_Off_v325")]
        [TestCase("12-13-000-000", "A16_Encrypt_Off_v384")]
        [TestCase("12-45-000-000", "A37_Encrypt_Off_v516")]
        [TestCase("12-48-000-000", "A47_ENCRYPT_OFF_svn_ver_680_P1_Debug")]
        [TestCase("15-19-000-000", "A47_ENCRYPT_OFF_svn_ver_680_P1_Debug")]
        public void TestLowestSerialNumber(string serial, string expectedVersion)
        {
            // Act
            var version = _firmwareVersionMapper.FromSerialNumber(serial);

            // Assert
            Assert.That(version, Is.EqualTo(expectedVersion));
        }
    }
}
