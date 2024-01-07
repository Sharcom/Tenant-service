using Microsoft.AspNetCore.Mvc;

namespace Logging_service.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : Controller
    {
        [HttpGet]
        [Route("/Health")]
        public IActionResult Health()
        {
            return Ok();
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return Ok("Hello world!");
        }
    }
}
