using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using QboxNext.Frontend.Blazor.Shared;
using QboxNext.Server.Domain;

namespace QboxNext.Frontend.Blazor.Client.Pages
{
    [Authorize]
    public partial class FetchData
    {
        public WeatherForecast[] forecasts;

        [Inject]
        public HttpClient Http { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }

            try
            {
                var q = new QboxDataQuery
                {
                    From = DateTime.Now,
                    To = DateTime.Now.AddDays(1),
                    Resolution = QboxQueryResolution.Hour,
                    AdjustHours = true
                };

                var x = Http.PostAsJsonAsync("api/data", q);
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }
    }
}
