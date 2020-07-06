using System;
using System.Threading.Tasks;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using QboxNext.Frontend.Blazor.Client.Services;
using QboxNext.Server.Domain;

namespace QboxNext.Frontend.Blazor.Client.Pages
{
    public partial class Electricity
    {
        [Inject]
        ChartService ChartService { get; set; }

        BarChart<int> BarChart;

        QboxDataQuery Query = new QboxDataQuery
        {
            From = DateTime.Now.Date,
            To = DateTime.Now.Date.AddDays(1),
            Resolution = QboxQueryResolution.Hour,
            AdjustHours = true
        };

        object BarChartOptionsObject => GetBarChartOptionObject("","");

        object GetBarChartOptionObject(string x, string y)
        {
            return new
            {
                // animation = new { duration = 0 },
                legend = new { display = true },
                scales = new
                {
                    xAxes = new[] { new { scaleLabel = new { display = true } } },
                    yAxes = new[] { new { scaleLabel = new { display = true } } }
                }
            };
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await ChartService.GetElectricityBarChartAsync(Query, BarChart);
                StateHasChanged();
            }

            // return base.OnAfterRenderAsync(firstRender);
        }
    }
}