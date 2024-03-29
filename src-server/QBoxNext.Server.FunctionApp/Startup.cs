﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QBoxNext.Server.FunctionApp;
using QBoxNext.Server.FunctionApp.Options;
using QBoxNext.Server.FunctionApp.Services;
using QBoxNext.Server.FunctionApp.Utils;

// https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
// https://stackoverflow.com/questions/54876798/how-can-i-use-the-new-di-to-inject-an-ilogger-into-an-azure-function-using-iwebj
[assembly: FunctionsStartup(typeof(Startup))]
namespace QBoxNext.Server.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configBuilder = new ConfigurationBuilder();

            string scriptRoot = AzureFunctionUtils.GetAzureWebJobsScriptRoot();

            // By default, only the "Values" section from local.settings.json is read.
            // In order to read more custom configuration sections, we need to use AddJsonFile(...)
            if (!string.IsNullOrEmpty(scriptRoot))
            {
                configBuilder.SetBasePath(scriptRoot).AddJsonFile("local.settings.json", optional: false, reloadOnChange: false);

                if (!AzureFunctionUtils.IsAzureEnvironment())
                {
                    // In case we are running locally, also add the development file.
                    configBuilder.SetBasePath(scriptRoot).AddJsonFile("local.settings.development.json", optional: false, reloadOnChange: false);
                }
            }

            configBuilder.AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            // Add Services
            builder.Services.AddSingleton(configuration);
            builder.Services.AddSingleton<INameResolver, CustomNameResolver>();
            builder.Services.AddScoped<IAzureTableStorageCleaner, AzureTableStorageCleaner>();

            // Logging
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddApplicationInsights();
            });

            // Options
            builder.Services.Configure<AzureTableStorageCleanerOptions>(configuration.GetSection("AzureTableStorageCleanerOptions"));
        }
    }
}