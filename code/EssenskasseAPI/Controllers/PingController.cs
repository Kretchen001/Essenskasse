using Microsoft.AspNetCore.Mvc;
using Serilog;
using SerilogTimings;

namespace EssenskasseAPI.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase {

        [HttpGet, Route("ping")]
        public IActionResult Ping() {
            using (Operation.Time("TaskDuration")) {
                Log.Information("Ping called.");
                return Ok();
            }
        }
    }
}
