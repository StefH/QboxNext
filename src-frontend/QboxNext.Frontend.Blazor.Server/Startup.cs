using System;
using System.Threading.Tasks;
using QboxNext.Frontend.Blazor.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace QboxNext.Frontend.Blazor.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        //public Startup(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //}

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // builder.AddApplicationInsightsSettings(developerMode: true);
            }

            Configuration = builder.Build();
            // HostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Auth0
            services.AddAuth0(options =>
            {
                var section = Configuration.GetSection("Auth0Options");

                options.JwtAuthority = section["JwtAuthority"];
                options.JwtAudience = section["JwtAudience"];

                options.Audience = section["Audience"];
                options.ClientId = section["ClientId"];
                options.ClientSecret = section["ClientSecret"];
                options.Domain = section["Domain"];
                options.Policies = section.GetSection("Policies").Get<List<string>>();
            });

            services.AddGrpc();
            services.AddControllers();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        private void AddDefaultJwtAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    Configuration.Bind("auth0", options);

                    options.Events = new JwtBearerEvents
                    {
                        OnForbidden = ctx =>
                        {
                            Console.WriteLine("OnForbidden");
                            return Task.CompletedTask;
                        },

                        OnChallenge = ctx =>
                        {
                            // invalid_token , https://github.com/auth0-samples/auth0-aspnetcore-webapi-samples/issues/13
                            Console.WriteLine("OnChallenge Error: " + ctx.Error);
                            Console.WriteLine("OnChallenge AuthenticateFailure.Message: " + ctx.AuthenticateFailure?.Message);
                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = ctx =>
                        {
                            Console.WriteLine("OnAuthenticationFailed: " + ctx.Exception.Message);
                            return Task.CompletedTask;
                        },

                        OnMessageReceived = ctx =>
                        {
                            Console.WriteLine("OnMessageReceived: " + ctx.Token);
                            return Task.CompletedTask;
                        },

                        OnTokenValidated = ctx =>
                        {
                            Console.WriteLine("OnTokenValidated: " + ctx.SecurityToken.Id);
                            return Task.CompletedTask;
                        }
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireAudience = true,
                        ValidAudience = options.Audience
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // UseAuthentication must come before UseAuthorization
            app.UseAuthorization();

            app.UseGrpcWeb(); // Must be added between UseRouting and UseEndpoints

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<UploadFileService>().EnableGrpcWeb();
                //endpoints.MapGrpcService<WeatherService>().EnableGrpcWeb();

                endpoints.MapGrpcService<CounterService>().EnableGrpcWeb();

                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}