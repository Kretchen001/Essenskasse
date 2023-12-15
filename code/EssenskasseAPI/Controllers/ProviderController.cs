using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;

namespace EssenskasseAPI.Controllers {
    
    [Route("api/provider")]
    [ApiController]
    public class ProviderController : Controller{

        ProviderDbController _providerController = new ProviderDbController();

        [HttpGet]
        [Route("requestAll")]
        public IActionResult GetAllProvider() {
            List<Provider> provider = _providerController.RequestProvider();

            if (provider is not null) {
                return Ok(JsonConvert.SerializeObject(provider));
            }
            else {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("testExist")]
        public IActionResult CheckExistProvider([FromHeader] Guid? guid, [FromHeader] string? designator) {
            Provider? provider = _providerController.ExistProvider(guid, designator);
            
            if (provider is not null) {
                if (provider.Designation != designator) {
                    return Conflict("Guid existiert, aber mit anderer Bezeichnung!");
                }
                return Ok(JsonConvert.SerializeObject(provider));
            }
            else {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("new")]
        public IActionResult AddProvider([FromHeader] string? name) {
            if (name is not null) {
                Provider newProvider = new Provider(name);
                if (_providerController.SaveProvider(newProvider)) {
                    return Ok(newProvider.Guid);
                }
                else {
                    return BadRequest("Provider konnte nicht eingefügt werden. Bitte Admin benachrichten.");
                }
            }
            else {
                return NoContent();
            }
        }

        [HttpPut]
        [Route("change")]
        public IActionResult ChangeProvider([FromHeader] Guid? guid, [FromHeader] string designation) {
            if (guid is not null) {
                Provider? result = _providerController.ExistProvider(guid ?? new Guid("00000000-0000-0000-0000"), null);
                if (result is not null) {
                    result.Designation = designation;
                    if (_providerController.SaveChangesAtProvider(result)) {
                        return Ok();
                    }
                    else {
                        return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                    }
                }
                else {
                    return BadRequest("Provider konnte nicht gefunden werden");
                }
            }
            else {
                return NoContent();
            }
        }

        [HttpDelete]
        [Route("remove")]
        public IActionResult RemoveProvider([FromHeader] Guid? guid) {
            if (guid is not null) {
                Provider? gefunden = _providerController.ExistProvider(guid ?? new Guid("00000000-0000-0000-0000"), null);
                if (gefunden is not null) {
                    _providerController.RemoveProvider(gefunden);
                    return Ok(JsonConvert.SerializeObject(gefunden));
                }
                else {
                    return BadRequest("Provider konnte nicht gefunden werden!");
                }
            }
            else {
                return NoContent();
            }
        }
    }
}

