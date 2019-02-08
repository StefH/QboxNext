using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using QboxNext.Server.Auth0.Authorization;
using QboxNext.Server.Auth0.Implementations;
using QboxNext.Server.Auth0.Interfaces;
using QboxNext.Server.Auth0.Options;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuth0(this IServiceCollection services, Action<Auth0Options> configureAuth0Options)
        {
            var auth0Options = new Auth0Options();
            configureAuth0Options(auth0Options);

            services.Configure(configureAuth0Options);

            services.AddOptions();

            services.AddServices();

            services.AddJwtBearerAuthentication(auth0Options);

            services.AddAuthorization(options =>
            {
                foreach (string policy in auth0Options.Policies)
                {
                    options.AddPolicy(policy, auth0Options.Domain);
                }
            });

            return services;
        }

        private static void AddServices(this IServiceCollection services)
        {
            // register the scope authorization handler
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            // register the IHttpClientFactory and IAuth0ClientFactory
            services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
            services.AddSingleton<IAuth0ClientFactory, Auth0ClientFactory>();
        }

        private static void AddJwtBearerAuthentication(this IServiceCollection services, Auth0Options auth0Options)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = auth0Options.JwtAuthority;
                options.Audience = auth0Options.JwtAudience;
            });
        }

        private static void AddPolicy(this AuthorizationOptions options, string policyName, string domain)
        {
            options.AddPolicy(policyName, policy => policy.Requirements.Add(new HasScopeRequirement(policyName, domain)));
        }
    }
}