﻿using CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.AzureTables;
using QboxNext.Logging;
using QboxNext.Server.Frontend.Options;
using QboxNext.Server.Infrastructure.Azure.Options;
using QBoxNext.Server.Business.DependencyInjection;
using System.Linq;

namespace QboxNext.Server.Frontend
{
    public class Startup
    {
        public bool UseSPA = false;

        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            if (UseSPA)
            {
                // In production, the Angular files will be served from this directory
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/dist";
                });
            }

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

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            if (UseSPA)
            {
                app.UseSpa(spa =>
                {
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501

                    spa.Options.SourcePath = "ClientApp";

                    if (HostingEnvironment.IsDevelopment())
                    {
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                });
            }
        }
    }
}