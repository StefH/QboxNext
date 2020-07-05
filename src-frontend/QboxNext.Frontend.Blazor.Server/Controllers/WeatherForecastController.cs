using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCore.Security.Auth0.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Frontend.Blazor.Shared;
using QboxNext.Server.Domain;

namespace QboxNext.Frontend.Blazor.Server.Controllers
{
    //[Authorize]
    [Authorize(QboxNextPolicies.ReadData)]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            string serialNumber = "";
            try
            {
                var claimsIdentity = (Auth0ClaimsIdentity)User.Identities.First(i => i is Auth0ClaimsIdentity);
                serialNumber = claimsIdentity.AppMetadata["qboxSerialNumber"].ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }            

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = serialNumber // Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
