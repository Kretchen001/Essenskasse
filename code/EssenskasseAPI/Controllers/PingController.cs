using Microsoft.AspNetCore.Mvc;

namespace EssenskasseAPI.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase {

        [HttpGet, Route("ping")]
        public IActionResult Ping() {
            return Ok();
        }
    }
}
