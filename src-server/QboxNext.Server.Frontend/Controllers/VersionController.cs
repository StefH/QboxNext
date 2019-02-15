using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;

namespace QboxNext.Server.Frontend.Controllers
{
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet("/api/version")]
        public ActionResult GetVersion()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            return Ok(new
            {
                Version = versionInfo.ProductVersion
            });
        }
    }
}