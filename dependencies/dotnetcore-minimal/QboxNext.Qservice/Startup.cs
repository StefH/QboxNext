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
using QboxNext.Qserver.Core.DataStore;
using QboxNext.Qserver.Core.Factories;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qservice.Mvc;

namespace QboxNext.Qservice
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
            services
                .AddCors()
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            QboxType qboxType = Configuration.GetValue("QboxType", QboxType.Duo);
            services.AddSingleton<IMiniRetriever>(new ConfiguredMiniRetriever(qboxType));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logFactory)
        {
            QboxNextLogProvider.LoggerFactory = logFactory;

            StorageProviderFactory.Register(StorageProvider.kWhStorage, typeof(kWhStorage));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCorsFromConfig();

            app.UseMvc();
        }
    }
}
