using CorrelationId;
using Microsoft.ApplicationInsights.Extensibility;
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
using QboxNext.Server.DataReceiver.Options;
using QboxNext.Server.DataReceiver.Telemetry;
using QboxNext.Server.Infrastructure.Azure.Options;
using System.Linq;

namespace QboxNext.Server.DataReceiver
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            if (env.IsDevelopment())
            {
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // https://github.com/Microsoft/ApplicationInsights-aspnetcore/wiki/Custom-Configuration
            services.AddSingleton<ITelemetryInitializer, QboxNextTelemetryInitializer>();
            services.AddApplicationInsightsTelemetry();

            // Add External services
            services
                .AddBusiness()
                .AddCorrelationId();

            // Configure
            services.Configure<AzureTableStorageOptions>(Configuration.GetSection("AzureTableStorageOptions"));
            services.Configure<AppOptions>(Configuration.GetSection("App"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory logFactory,
            IOptions<AzureTableStorageOptions> azureTableStorageOptions,
            IOptions<AppOptions> appOptions
        )
        {
            var logger = logFactory.CreateLogger<Startup>();

            // Update the ConnectionString from the TableStorageTarget and AsyncTargetWrapper[TableStorageTarget]
            var target = LogManager.Configuration.AllTargets.OfType<TableStorageTarget>().FirstOrDefault();
            if (target != null)
            {
                logger.LogInformation("Updating target.ConnectionString to {ConnectionString}", azureTableStorageOptions.Value.ConnectionString);
                target.ConnectionString = azureTableStorageOptions.Value.ConnectionString;
            }
            LogManager.ReconfigExistingLoggers();

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