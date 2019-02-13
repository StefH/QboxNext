using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QboxNext.Qservice.Mvc
{
    internal static class CorsSupportExtensions
    {
        /// <summary>
        /// Adds a CORS middleware to your web application pipeline to allow cross domain requests.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" /> passed to your Configure method.</param>
        /// <param name="options">A delegate with which to configure the <see cref="CorsPolicyOptions" />.</param>
        /// <returns>The original app parameter.</returns>
        public static IApplicationBuilder UseCorsFromConfig(this IApplicationBuilder app, Action<CorsPolicyOptions> options = null)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var corsOpts = new CorsPolicyOptions();
            config.Bind("CorsPolicy", corsOpts);
            options?.Invoke(corsOpts);

            if (corsOpts.AllowedOrigins.Any())
            {
                var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger<CorsPolicyOptions>();
                logger.LogInformation("Allowing CORS for origins = {Origins}, methods = {Methods}", corsOpts.AllowedOrigins, corsOpts.Methods);

                app.UseCors(builder =>
                {
                    builder
                        .WithOrigins(corsOpts.AllowedOrigins)
                        .AllowAnyHeader();
                    if (corsOpts.Methods.Any())
                    {
                        builder.WithMethods(corsOpts.Methods);
                    }
                    else
                    {
                        builder.AllowAnyHeader();
                    }
                });
            }

            return app;
        }
    }
}