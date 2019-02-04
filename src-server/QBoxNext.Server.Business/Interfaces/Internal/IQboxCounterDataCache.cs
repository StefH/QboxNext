using QboxNext.Server.Domain;
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace QBoxNext.Server.Business.Interfaces.Internal
{
    internal interface IQboxCounterDataCache
    {
        Task<QboxPagedDataQueryResult<QboxCounterData>> GetOrCreateAsync([NotNull] QboxDataQuery query, [NotNull] Func<Task<QboxPagedDataQueryResult<QboxCounterData>>> getDataFunc);
    }
}