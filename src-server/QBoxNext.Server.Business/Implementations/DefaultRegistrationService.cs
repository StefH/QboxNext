using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QBoxNext.Server.Business.Interfaces.Public;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class DefaultRegistrationService : IRegistrationService
    {
        private readonly IAzureTablesService _azureTablesService;

        public DefaultRegistrationService([NotNull] IAzureTablesService azureTablesService)
        {
            Guard.NotNull(azureTablesService, nameof(azureTablesService));

            _azureTablesService = azureTablesService;
        }

        public Task<bool> IsValidRegistrationAsync(string serialNumber)
        {
            return _azureTablesService.IsValidRegistrationAsync(serialNumber);
        }
    }
}