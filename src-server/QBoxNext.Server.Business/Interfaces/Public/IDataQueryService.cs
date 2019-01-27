using JetBrains.Annotations;
using QboxNext.Server.Domain;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Interfaces.Public
{
    public interface IDataQueryService
    {
        Task<object> QueryAsync([NotNull] QboxDataQuery query);
    }
}