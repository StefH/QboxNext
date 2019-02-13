using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace QboxNext.Storage
{
    [TestFixture]
    public class StorageProviderFactoryTest
    {
        private StorageProviderFactory _sut;
        private StorageProviderContext _storageContext;

        private class StorageProviderStub : IStorageProvider {
            public StorageProviderStub(StorageProviderContext context)
            {
                Context = context;
            }

            public StorageProviderContext Context { get; private set; }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public decimal Sum(DateTime begin, DateTime end, Unit eenheid)
            {
                throw new NotImplementedException();
            }

            public bool GetRecords(DateTime inBegin, DateTime inEnd, Unit inUnit, IList<SeriesValue> ioSlots, bool inNegate)
            {
                throw new NotImplementedException();
            }

            public Record GetValue(DateTime measureTime)
            {
                throw new NotImplementedException();
            }

            public Record SetValue(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit, decimal inEurocentsPerUnit, Record inRunningTotal = null)
            {
                throw new NotImplementedException();
            }

            public void ReinitializeSlots(DateTime inFrom)
            {
                throw new NotImplementedException();
            }

            public Record FindPrevious(DateTime inMeasurementTime)
            {
                throw new NotImplementedException();
            }
        }

        private class StorageProviderStubWithMissingCtorParams : StorageProviderStub
        {
            public StorageProviderStubWithMissingCtorParams() : base(null)
            {
            }
        }


		[SetUp]
        public void SetUp()
        {
            _storageContext = new StorageProviderContext
            {
                SerialNumber = "00-00-000-000",
                CounterId = 1,
                Precision = Precision.mWh
            };

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(_storageContext)
                .BuildServiceProvider();

            _sut = new StorageProviderFactory(new NullLogger<StorageProviderFactory>(), serviceProvider, typeof(StorageProviderStub));
        }

        [Test]
        public void When_getting_provider_with_valid_context_should_return_instance()
        {
            // Act
            IStorageProvider storage = _sut.GetStorageProvider(_storageContext);

            // Assert
            storage
                .Should()
                .NotBeNull()
                .And
                .BeOfType<StorageProviderStub>()
                .Which.Context
                .Should()
                .Be(_storageContext);
        }

        [Test]
        public void When_creating_factory_with_invalid_provider_type_should_throw()
        {
            Type invalidProviderType = typeof(object);

            // Act
            Action act = () => new StorageProviderFactory(new NullLogger<StorageProviderFactory>(), new ServiceCollection().BuildServiceProvider(), invalidProviderType);

            // Assert
            ArgumentException ex = act.Should().Throw<ArgumentException>().Which;
            ex.Message.Should().StartWith($"The specified type does not implement {typeof(IStorageProvider).FullName}.");
            ex.ParamName.Should().Be("storageProviderType");
        }

        [Test]
        public void When_getting_provider_cant_be_created_should_throw()
        {
            Type invalidProviderType = typeof(StorageProviderStubWithMissingCtorParams);
            _sut = new StorageProviderFactory(new NullLogger<StorageProviderFactory>(), new ServiceCollection().BuildServiceProvider(), invalidProviderType);

            // Act
            Action act = () => _sut.GetStorageProvider(new StorageProviderContext());

            // Assert
            StorageException ex = act.Should().Throw<StorageException>().Which;
            ex.Message.Should().Be($"Unable to create storage provider {typeof(StorageProviderStubWithMissingCtorParams).FullName}.");
            ex.InnerException.Should().BeOfType<InvalidOperationException>();
        }

		[Test]
        public void When_registering_without_serviceProvider()
        {
            // Act
            Action act = () => new StorageProviderFactory(new NullLogger<StorageProviderFactory>(), null, typeof(object));

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName
                .Should()
                .Be("serviceProvider");
        }

        [Test]
        public void When_registering_without_type()
        {
            // Act
            Action act = () => new StorageProviderFactory(new NullLogger<StorageProviderFactory>(), new ServiceCollection().BuildServiceProvider(), null);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName
                .Should()
                .Be("storageProviderType");
        }

        [Test]
        public void When_getting_provider_without_context()
        {
            // Act
            Action act = () => _sut.GetStorageProvider(null);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName
                .Should()
                .Be("context");
        }
    }
}
