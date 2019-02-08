using JetBrains.Annotations;
using QboxNext.Server.Domain;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Interfaces.Public
{
    public interface IDataQueryService
    {
        Task<QboxPagedDataQueryResult<QboxCounterData>> QueryAsync([NotNull] string serialNumber, [NotNull] QboxDataQuery query);
    }
}