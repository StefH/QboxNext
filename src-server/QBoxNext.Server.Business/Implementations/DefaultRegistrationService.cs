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

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRegistrationService"/> class.
        /// </summary>
        /// <param name="azureTablesService">The azure tables service.</param>
        public DefaultRegistrationService([NotNull] IAzureTablesService azureTablesService)
        {
            Guard.NotNull(azureTablesService, nameof(azureTablesService));

            _azureTablesService = azureTablesService;
        }

        /// <inheritdoc cref="IRegistrationService.IsValidRegistrationAsync(string)"/>
        public Task<bool> IsValidRegistrationAsync(string serialNumber)
        {
            return _azureTablesService.IsValidRegistrationAsync(serialNumber);
        }
    }
}