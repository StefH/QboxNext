using Microsoft.AspNetCore.Mvc;

namespace QboxNext.WebApi.Docker.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("/")]
        public string Get()
        {
            return "ok";
        }
    }
}
