using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WeatherApi
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
            // services.AddControllers();


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

            // Configure AddControllers with AuthorizationPolicyBuilder (stef)
            services.AddControllers(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });


            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.Authority = "https://stef-heyenrath.eu.auth0.com/";
            //        options.Audience = "https://qboxnext.web.nl";
            //        options.

            //        //options.
            //        //options.

            //        //options.TokenValidationParameters = new TokenValidationParameters
            //        //{
            //        //    NameClaimType = "name"
            //        //};
            //    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(config =>
            {
                config.AllowAnyOrigin();
                config.AllowAnyMethod();
                config.AllowAnyHeader();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
