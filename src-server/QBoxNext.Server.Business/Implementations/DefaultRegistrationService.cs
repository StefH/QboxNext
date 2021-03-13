using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QBoxNext.Server.Business.Interfaces.Public;
using System.Threading.Tasks;
using QboxNext.Server.Domain;

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

        /// <inheritdoc cref="IRegistrationService.GetQboxRegistrationDetailsAsync(string)"/>
        public Task<QboxRegistrationDetails> GetQboxRegistrationDetailsAsync(string serialNumber)
        {
            return _azureTablesService.GetQboxRegistrationDetailsAsync(serialNumber);
        }
    }
}