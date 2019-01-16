using QboxNext.Qserver.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace QBoxNext.Business.Interfaces.Internal
{
    internal interface IStorageProviderAsync : IStorageProvider
    {
        Task StoreValueAsync(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit);
    }
}