using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using QboxNext.Extensions.Models.Public;

namespace QboxNext.Extensions.Interfaces.Public
{
    public interface ICounterStoreService
    {
        Task StoreAsync(Guid correlationId, [NotNull] CounterData counterData);
    }
}