using System;
using System.Net.Http;
using System.Threading.Tasks;
using QboxNext.Frontend.Blazor.Client.Services;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace QboxNext.Frontend.Blazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            // Blazorise
            builder.Services
                .AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true;
                })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            builder.Services.AddHttpClient("QboxNext.Frontend.Blazor.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("QboxNext.Frontend.Blazor.ServerAPI"));

            builder.Services.AddAuth0Authentication(options =>
            {
                builder.Configuration.Bind("auth0", options.ProviderOptions);

                // The callback url is : https://localhost:5001/authentication/login-callback
                // Make sure to add this to the Auth0 allowed callback urls !
            });

            builder.Services.AddGrpcBearerTokenProvider();

            builder.Services.AddScoped(services =>
            {
                // Create a channel with a GrpcWebHandler that is addressed to the backend server. GrpcWebText is used because server streaming requires it.
                var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
                return GrpcChannel.ForAddress(builder.HostEnvironment.BaseAddress, new GrpcChannelOptions { HttpHandler = httpHandler });
            });

            await builder.Build().RunAsync();
        }
    }
}