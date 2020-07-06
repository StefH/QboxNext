using System.ServiceModel;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using QboxNext.Server.Domain;

namespace QboxNext.Frontend.Blazor.Shared
{
    [ServiceContract(Name = "QboxNext.DataQueryServiceContract")]
    public interface IDataQueryServiceContract
    {
        [OperationContract]
        Task<QboxPagedCounterDataResult> GetCounterDataAsync(QboxDataQuery request, CallContext context = default);
    }
}
