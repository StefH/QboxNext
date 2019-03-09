using CorrelationId;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddMvc();

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
            AddNLogTableStorageTarget(azureTableStorageOptions);

            // TODO : this needs to be in place until correct DI is added to QboxNext
            QboxNextLogProvider.LoggerFactory = logFactory;

            app.UseCorrelationId(new CorrelationIdOptions
            {
                UpdateTraceIdentifier = true,
                IncludeInResponse = false,
                UseGuidForCorrelationId = true
            });

            app.UseMvc();
        }

        private void AddNLogTableStorageTarget(IOptions<AzureTableStorageOptions> azureTableStorageOptions)
        {
            var section = Configuration.GetSection("Logging").GetSection("TableStorageTarget");

            var target = new TableStorageTarget
            {
                Name = "AzureTable",
                MachineName = section["MachineName"],
                TableName = section["TableName"],
                ConnectionString = azureTableStorageOptions.Value.ConnectionString,
                CorrelationId = "${aspnet-TraceIdentifier}",
                Layout = section["Layout"]
            };

            LogManager.Configuration.AddTarget(target);
            LogManager.Configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, target);

            LogManager.ReconfigExistingLoggers();
        }
    }
}