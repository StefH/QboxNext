using JetBrains.Annotations;
using QboxNext.Server.Domain;
using System;
using System.Threading.Tasks;

namespace QboxNext.Server.Infrastructure.Azure.Interfaces.Public
{
    public interface IAzureTablesQueryService
    {
        /// <summary>
        /// Checks if the SerialNumber is a valid registration.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns>true/false</returns>
        Task<bool> IsValidRegistrationAsync([CanBeNull] string serialNumber);

        Task<QboxPagedDataQueryResult<QboxCounterData>> QueryDataAsync([NotNull] string serialNumber, DateTime from, DateTime to, QboxQueryResolution resolution, bool adjustHours);
    }
}