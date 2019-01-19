using System;
using Microsoft.AspNetCore.Mvc;

namespace QboxNext.WebApi.Docker.Controllers
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