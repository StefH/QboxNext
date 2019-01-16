﻿using Microsoft.AspNetCore.Mvc;
using System;

namespace QboxNext.WebApi.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet("/status")]
        public string Get()
        {
            return $"QboxNext.WebApi.Docker is running @ {DateTime.UtcNow}";
        }
    }
}