﻿using Microsoft.AspNetCore.Mvc;
using System;

namespace QboxNext.Server.WebApi.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet(@"/status/{productNumber:regex(^\d{{4}}-\d{{4}}-\d{{4}}$)}/{serialNumber:regex(^\d{{2}}-\d{{2}}-\d{{3}}-\d{{3}}$)}")]
        public string Get()
        {
            return $"QboxNext.WebApi is running @ {DateTime.UtcNow}";
        }
    }
}