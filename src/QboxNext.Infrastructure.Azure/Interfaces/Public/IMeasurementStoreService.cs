using JetBrains.Annotations;
using QboxNext.Domain;
using QboxNext.Infrastructure.Azure.Models.Public;

namespace QboxNext.Infrastructure.Azure.Interfaces.Public
{
    public interface IMeasurementStoreService
    {
        StoreResult Store([NotNull] Measurement measurement);
    }
}