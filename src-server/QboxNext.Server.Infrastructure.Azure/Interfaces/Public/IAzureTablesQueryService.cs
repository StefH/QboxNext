using JetBrains.Annotations;
using QboxNext.Server.Domain;
using System;
using System.Threading.Tasks;

namespace QboxNext.Server.Infrastructure.Azure.Interfaces.Public
{
    public interface IAzureTablesQueryService
    {
        /// <summary>
        /// Get QboxRegistrationDetails for a SerialNumber.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns>QboxRegistrationDetails if found, else null</returns>
        Task<QboxRegistrationDetails> GetQboxRegistrationDetailsAsync([CanBeNull] string serialNumber);

        Task<QboxPagedDataQueryResult<QboxCounterData>> QueryDataAsync([NotNull] string serialNumber, DateTime from, DateTime to, QboxQueryResolution resolution, bool adjustHours);
    }
}