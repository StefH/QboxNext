using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace QboxNext.Qserver.Tests
{
    [Category("Integration")]
    [TestFixture]
    public class QserverIntegrationTests
    {
        private readonly string _qserverMessageEndpoint = "/device/qbox/6618-1400-0200/15-46-002-442";
        private HttpClient _httpClient;
        private string _messagePayload;

        [SetUp]
        public void Setup()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:80") };
            _messagePayload = @"
FAFB070DABB7440780/KFM5KAIFA-METER 1-3:0.2.8(40) 0-0:1.0.0(000102045905W) 
0-0:96.1.1(4530303033303030303030303032343133) 1-0:1.8.1(000001.011*kWh) 
1-0:1.8.2(000000.000*kWh) 1-0:2.8.1(000000.000*kWh) 
1-0:2.8.2(000000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(00.034*kW) 
1-0:2.7.0(00.000*kW) 0-0:17.0.0(999.9*kW) 0-0:96.3.10(1) 0-0:96.7.21(00073) 
0-0:96.7.9(00020) 1-0:99.97.0(3)(0-0:96.7.19)(000124235657W)(0000003149*s)(000124225935W)(0000000289*s)(000101000001W)(2147483647*s) 
1-0:32.32.0(00005) 1-0:52.32.0(00006) 1-0:72.32.0(00001) 1-0:32.36.0(00000) 
1-0:52.36.0(00000) 1-0:72.36.0(00000) 0-0:96.13.1() 0-0:96.13.0() 1-0:31.7.0(000*A) 
1-0:51.7.0(000*A) 1-0:71.7.0(000*A) 1-0:21.7.0(00.034*kW) 1-0:22.7.0(00.000*kW) 1-0:41.7.0(00.000*kW) 
1-0:42.7.0(00.000*kW) 1-0:61.7.0(00.000*kW) 1-0:62.7.0(00.000*kW) 0-1:24.1.0(003) 
0-1:96.1.0(4730303131303033303832373133363133) 0-1:24.2.1(000102043601W)(62869.839*m3) 0-1:24.4.0(1) !583C
";
        }

        [Test]
        public async Task When_sending_a_new_message_it_should_return_an_ok_result()
        {
            // Act
            var response = await _httpClient.PostAsync(_qserverMessageEndpoint, new StringContent(_messagePayload));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentLength.Should().Be(14);
            response.Content.Headers.ContentType.Should().BeEquivalentTo(new MediaTypeHeaderValue("text/plain") { CharSet = "utf-8" });
        }
    }
}