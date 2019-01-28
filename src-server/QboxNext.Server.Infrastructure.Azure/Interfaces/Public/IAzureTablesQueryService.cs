using JetBrains.Annotations;
using QboxNext.Server.Domain;
using System;
using System.Collections.Generic;
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

        Task<PagedQueryResult<QboxCounterDataValue>> QueryDataAsync([NotNull] string serialNumber, [NotNull] IList<int> counterIds, DateTime from, DateTime to, QueryResolution resolution);
    }
}