using Microsoft.AspNetCore.Mvc;
using QboxNext.Server.Common.Utils;

namespace QboxNext.Server.Frontend.Controllers
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