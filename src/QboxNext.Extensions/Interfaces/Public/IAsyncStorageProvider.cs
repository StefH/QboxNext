using System;
using System.Threading.Tasks;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface IAsyncStorageProvider : IStorageProvider
    {
        Task StoreValueAsync(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit);
    }
}