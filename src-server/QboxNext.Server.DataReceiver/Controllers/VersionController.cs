using Microsoft.AspNetCore.Mvc;
using QboxNext.Server.Common.Utils;

namespace QboxNext.Server.DataReceiver.Controllers
{
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet(@"/version/{productNumber:regex(^\d{{4}}-\d{{4}}-\d{{4}}$)}/{serialNumber:regex(^\d{{2}}-\d{{2}}-\d{{3}}-\d{{3}}$)}")]
        public ActionResult GetVersion()
        {
            return Ok(AssemblyUtils.GetVersion());
        }
    }
}