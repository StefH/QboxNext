using System;
using Microsoft.AspNetCore.Mvc;

namespace QboxNext.Server.WebApi.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet("/status")]
        public string Get()
        {
            return $"QboxNext.WebApi is running @ {DateTime.UtcNow}";
        }
    }
}