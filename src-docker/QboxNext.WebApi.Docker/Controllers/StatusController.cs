using Microsoft.AspNetCore.Mvc;

namespace QboxNext.WebApi.Docker.Controllers
{
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet("/")]
        public string Get()
        {
            return "QboxNext.WebApi.Docker is running";
        }
    }
}
