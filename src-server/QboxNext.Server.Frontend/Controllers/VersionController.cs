using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Reflection;

namespace QboxNext.Server.Frontend.Controllers
{
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet("/api/version")]
        public ActionResult GetVersion()
        {
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes().ToArray();
            var copyright = (AssemblyCopyrightAttribute)attributes.First(a => a is AssemblyCopyrightAttribute);
            var version = (AssemblyInformationalVersionAttribute)attributes.First(a => a is AssemblyInformationalVersionAttribute);

            return Ok(new
            {
                copyright.Copyright,
                version.InformationalVersion
            });
        }
    }
}