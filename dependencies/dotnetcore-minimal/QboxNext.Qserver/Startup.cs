using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QboxNext.Logging;
using QboxNext.Model.Classes;
using QboxNext.Model.Interfaces;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Extensions;
using QboxNext.Qserver.Classes;
using QboxNext.Qserver.Core.DataStore;
using QboxNext.Qserver.Core.Factories;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Utils;

namespace QboxNext.Qserver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            QboxType qboxType = Configuration.GetValue("QboxType", QboxType.Duo);
            services.AddSingleton<IMiniRetriever>(new ConfiguredMiniRetriever(qboxType));
            services.AddSingleton<IQboxDataDumpContextFactory, QboxDataDumpContextFactory>();

            services.AddParsers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logFactory)
        {
            QboxNextLogProvider.LoggerFactory = logFactory;

            StorageProviderFactory.Register(StorageProvider.kWhStorage, typeof(kWhStorage));
            ClientRepositories.Queue = new MemoryQueue<string>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
