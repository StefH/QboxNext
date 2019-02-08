using JetBrains.Annotations;
using QboxNext.Server.Domain;
using System;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Interfaces.Internal
{
    internal interface IQboxCounterDataCache
    {
        Task<QboxPagedDataQueryResult<QboxCounterData>> GetOrCreateAsync(
            [NotNull] string serialNumber,
            [NotNull] QboxDataQuery query,
            [NotNull] Func<Task<QboxPagedDataQueryResult<QboxCounterData>>> getDataFunc);
    }
}