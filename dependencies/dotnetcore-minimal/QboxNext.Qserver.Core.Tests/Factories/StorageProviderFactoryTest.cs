using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using QboxNext.Logging;
using QboxNext.Qserver.Core.DataStore;
using QboxNext.Qserver.Core.Exceptions;
using QboxNext.Qserver.Core.Factories;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.Model.Factories
{
    [TestFixture]
    [NonParallelizable]
    public class StorageProviderFactoryTest
    {
        [SetUp]
        public void Init()
        {
            // Setup static logger factory
            QboxNextLogProvider.LoggerFactory = new LoggerFactory();
        }

        [Test]
        public void WhenRegisteringStorageProviderItShouldReturnTheInstanceOnGetProviderTest()
        {
            // arrange
            StorageProviderFactory.Register(StorageProvider.kWhStorage, typeof(kWhStorage), true);

            // act
            var storage = StorageProviderFactory.GetStorageProvider(false, StorageProvider.kWhStorage, "00-00-000-000", @".\Temp", 1, Precision.mWh);

            // assert
            Assert.IsNotNull(storage);
        }

        [Test]
        public void WhenRegisteringInCorrectTypeItShouldThrowTest()
        {
            Assert.Throws<StorageException>(
                () => StorageProviderFactory.Register(StorageProvider.kWhStorage, typeof(int), true));
        }

        [Test]
        public void WhenGettingWithNullShouldThrowTest()
        {
            Assert.Throws<ArgumentNullException>(
                () => StorageProviderFactory.GetStorageProvider(false, StorageProvider.kWhStorage, null, null, 0, Precision.kWh));
        }

        [Test]
        public void WhenGettingWitDataStorehNullShouldThrowTest()
        {
            // arrange  
            StorageProviderFactory.Register(StorageProvider.kWhStorage, typeof(kWhStorage), true);

            // act & assert
            Assert.Throws<ArgumentNullException>(
                () => StorageProviderFactory.GetStorageProvider(false, StorageProvider.kWhStorage, "00-00-000-000", null, 0, Precision.kWh));
        }
    }
}
