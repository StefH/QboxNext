using Microsoft.AspNetCore.Mvc;
using QboxNext.Server.Common.Utils;

namespace QboxNext.Frontend.Blazor.Server.Controllers
{
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet("/api/version")]
        public ActionResult GetVersion()
        {
            return Ok(AssemblyUtils.GetVersion());
        }
    }
}