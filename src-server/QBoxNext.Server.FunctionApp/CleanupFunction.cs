using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using QBoxNext.Server.FunctionApp.Services;

namespace QBoxNext.Server.FunctionApp
{
    public sealed class CleanupFunction
    {
        private readonly IAzureTableStorageCleaner _service;
        private readonly ILogger<CleanupFunction> _logger;

        public CleanupFunction(ILogger<CleanupFunction> logger, IAzureTableStorageCleaner service)
        {
            _logger = logger;
            _service = service;
        }

        [FunctionName("CleanupQboxStates")]
        public async Task CleanupQboxStatesAsync([TimerTrigger("%AzureTableStorageCleanerOptions:StatesTableCronExpression%")] TimerInfo myTimer)
        {
            _logger.LogInformation("CleanupQboxStates function executed at: {now}", DateTime.Now);

            await _service.CleanupStatesAsync();
        }
    }
}