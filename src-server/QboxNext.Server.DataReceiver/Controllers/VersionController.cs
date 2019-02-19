using Microsoft.AspNetCore.Mvc;
using QboxNext.Server.Common.Utils;

namespace QboxNext.Server.DataReceiver.Controllers
{
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet("/version")]
        public ActionResult GetVersion()
        {
            return Ok(AssemblyUtils.GetVersion());
        }
    }
}