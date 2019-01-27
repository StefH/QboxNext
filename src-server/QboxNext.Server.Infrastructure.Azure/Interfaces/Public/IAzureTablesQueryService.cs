using JetBrains.Annotations;
using QboxNext.Server.Infrastructure.Azure.Models.Public;
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

        Task<QueryResult> QueryDataAsync([NotNull] string serialNumber, [NotNull] int[] counterIds, [CanBeNull] DateTime? from, [CanBeNull] DateTime? to);
    }
}