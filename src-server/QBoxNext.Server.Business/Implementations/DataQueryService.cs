using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QBoxNext.Server.Business.Interfaces.Internal;
using QBoxNext.Server.Business.Interfaces.Public;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class DataQueryService : IDataQueryService
    {
        private readonly IAzureTablesService _azureTablesService;
        private readonly IQboxCounterDataCache _cache;

        public DataQueryService([NotNull] IAzureTablesService azureTablesService, [NotNull] IQboxCounterDataCache cache)
        {
            Guard.NotNull(azureTablesService, nameof(azureTablesService));
            Guard.NotNull(cache, nameof(cache));

            _azureTablesService = azureTablesService;
            _cache = cache;
        }

        /// <inheritdoc cref="IDataQueryService.QueryAsync(QboxDataQuery)"/>
        public async Task<QboxPagedDataQueryResult<QboxCounterData>> QueryAsync(QboxDataQuery query)
        {
            Guard.NotNull(query, nameof(query));

            return await _cache.GetOrCreateAsync(query, () => _azureTablesService.QueryDataAsync(query.SerialNumber, query.From, query.To, query.Resolution, query.AddHours));
        }
    }
}