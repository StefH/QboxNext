using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using QboxNext.Core.Dto;
using QboxNext.Qservice.Classes;

namespace QboxNext.Qservice.Tests
{
    [Category("Smoke")]
    [TestFixture]
    public class QserviceSmokeTests
    {
        private readonly string _qserviceMessageEndpoint = $"api/getseries?sn=15-46-002-442&from={DateTime.Today.AddDays(-1):yyyy-MM-dd}&to={DateTime.Today:yyyy-MM-dd}";
        private HttpClient _httpClient;

        [SetUp]
        public void Setup()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5002") };
        }

        [Test]
        public async Task When_sending_a_new_message_it_should_return_an_ok_result()
        {
            // Act
            var response = await _httpClient.GetAsync(_qserviceMessageEndpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.Should().BeEquivalentTo(new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" });

            var stream = await response.Content.ReadAsStreamAsync();
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                var serieResult = new JsonSerializer().Deserialize<SerieResult>(jr);
                serieResult.Result.Should().BeTrue();
                serieResult.Data.Should()
                    .BeEquivalentTo(new List<Serie>
                    {
                        new Serie
                        {
                            Data = Enumerable.Repeat((decimal?)0, 288).ToList(),
                            EnergyType = DeviceEnergyType.NetLow
                        },
                        new Serie
                        {
                            Data = Enumerable.Repeat((decimal?)0, 288).ToList(),
                            EnergyType = DeviceEnergyType.Consumption
                        },
                        new Serie
                        {
                            Data = Enumerable.Repeat((decimal?)0, 288).ToList(),
                            EnergyType = DeviceEnergyType.NetHigh
                        },
                        new Serie
                        {
                            Data = Enumerable.Repeat((decimal?)0, 24).ToList(),
                            EnergyType = DeviceEnergyType.Gas
                        },
                        new Serie
                        {
                            Data = Enumerable.Repeat((decimal?)0, 288).ToList(),
                            EnergyType = DeviceEnergyType.Generation
                        }
                    });
            }
        }

        private class SerieResult
        {
            public bool Result { get; set; }
            public IList<Serie> Data { get; set; }
        }
    }
}