using EssenskasseAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SerilogTimings;

namespace EssenskasseAPI.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase {

        private readonly Db _db;

        public PingController(IConfiguration configuration) {
            _db = new Db(configuration);
        }

        [HttpGet, Route("ping")]
        public IActionResult Ping() {
            using (Operation.Time("TaskDuration")) {
                Log.Information("Ping called.");
                return Ok();
            }
        }

        [HttpGet, Route("pingDb")]
        public IActionResult PingWithDb() {
            using(Operation.Time("TaskDuration")) {
                try {
                    // Die Tabelle Test existiert seit Version 1.2.0 nicht mehr (siehe update_v1-1-0_zu_1-2-0.sql).
                    _db.Query("SELECT 1;");

                    return Ok();
                }
                catch (Exception ex) {
                    return BadRequest(ex.ToString());
                }
            }
        }
    }
}
