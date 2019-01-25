using CorrelationId;
using Exceptionless;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.AzureTables;
using QboxNext.Logging;
using QboxNext.Server.Infrastructure.Azure.Options;
using QboxNext.Server.WebApi.Options;
using QBoxNext.Server.Business.DependencyInjection;
using System.Linq;

namespace QboxNext.Server.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Add External services
            services
                .AddBusiness()
                .AddCorrelationId();

            // Configure
            services.Configure<AzureTableStorageOptions>(Configuration.GetSection("AzureTableStorageOptions"));
            services.Configure<AppOptions>(Configuration.GetSection("AzureTableStorageOptions"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory logFactory,
            IOptions<AzureTableStorageOptions> azureTableStorageOptions,
            IOptions<AppOptions> appOptions
        )
        {
            if (!string.IsNullOrEmpty(appOptions.Value.ExceptionLessApiKey))
            {
                app.UseExceptionless(appOptions.Value.ExceptionLessApiKey);
            }

            // Update the ConnectionString from the TableStorageTarget
            if (LogManager.Configuration.AllTargets.FirstOrDefault(t => t is TableStorageTarget) is TableStorageTarget target)
            {
                target.ConnectionString = azureTableStorageOptions.Value.ConnectionString;
                LogManager.ReconfigExistingLoggers();
            }

            // TODO : this needs to be in place until correct DI is added to QboxNext
            QboxNextLogProvider.LoggerFactory = logFactory;

            app.UseCorrelationId(new CorrelationIdOptions
            {
                UpdateTraceIdentifier = true,
                IncludeInResponse = true,
                UseGuidForCorrelationId = true
            });

            app.UseMvc();
        }
    }
}