using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace EssenskasseAPI.Controllers {

    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase {

        readonly TokenDbController tokenDbController = new TokenDbController();

        [HttpGet, Route("requestNewAccessToken")]
        public IActionResult RequestNewAccessToken() {
            Log.Information("RequestNewAccessToken called");
            return Ok(JsonConvert.SerializeObject(tokenDbController.CreateNewAccessToken()));
        }

        [HttpGet, Route("checkAccessToken")]
        public IActionResult CheckAccessToken([FromHeader] Guid? accessToken, [FromQuery] bool s) {
            if (s) {
                Log.Information("CheckAccessToken called");
                if (accessToken is null) {
                    return NoContent();
                }
                if (tokenDbController.CheckAccessToken(accessToken ?? new Guid("00000000-0000-0000-0000-000000000000"))) {
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
