using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QBoxNext.Server.Business.Interfaces.Public;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class DataQueryService : IDataQueryService
    {
        private readonly IAzureTablesService _azureTablesService;

        public DataQueryService([NotNull] IAzureTablesService azureTablesService)
        {
            Guard.NotNull(azureTablesService, nameof(azureTablesService));

            _azureTablesService = azureTablesService;
        }

        /// <inheritdoc cref="IDataQueryService.QueryAsync(QboxDataQuery)"/>
        public async Task<object> QueryAsync(QboxDataQuery query)
        {
            Guard.NotNull(query, nameof(query));

            var result = await _azureTablesService.QueryDataAsync(query.SerialNumber, query.From, query.To, query.Resolution, query.AddHours);

            return result;
        }
    }
}