using Microsoft.AspNetCore.Mvc;
using System;

namespace QboxNext.Server.DataReceiver.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet(@"/status/{productNumber:regex(^\d{{4}}-\d{{4}}-\d{{4}}$)}/{serialNumber:regex(^\d{{2}}-\d{{2}}-\d{{3}}-\d{{3}}$)}")]
        public ActionResult Get()
        {
            return Ok($"QboxNext.Server.DataReceiver is running @ {DateTime.UtcNow:s}");
        }
    }
}