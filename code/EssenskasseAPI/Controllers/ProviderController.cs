using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;

namespace EssenskasseAPI.Controllers {
    
    [ApiController, Route("api/provider")]
    public class ProviderController : ControllerBase {

        readonly ProviderDbController _providerController = new ProviderDbController();

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllProvider() {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetAllProvider called");
                List<Provider> provider = _providerController.RequestProvider();

                if (provider.Count != 0) {
                    provider.ForEach(x => x.Designation = Uri.EscapeDataString(x.Designation));
                    Log.Information(provider.Count + " Person requested");
                    return Ok(JsonConvert.SerializeObject(provider));
                }
                else {
                    Log.Warning("No Data response (GetAllProvider)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("testExist")]
        public IActionResult CheckExistProvider([FromHeader] Guid? guid, [FromHeader] string? designator) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("CheckExistProvider called");
                Provider? provider = _providerController.ExistProvider(guid, designator);

                if (provider is not null) {
                    if (provider.Designation != designator) {
                        Log.Information($"{guid} with other {provider.Designation} found and not {designator}");
                        return Conflict("Guid existiert, aber mit anderer Bezeichnung!");
                    }
                    Log.Information("Provider exist");
                    return Ok(JsonConvert.SerializeObject(provider));
                }
                else {
                    Log.Warning(guid.ToString() + " not found.");
                    return NoContent();
                }
            }
        }

        [HttpPost, Route("new")]
        public IActionResult AddProvider([FromHeader] string? name) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddProvider called");
                if (name is not null) {
                    name = Uri.UnescapeDataString(name);
                    Provider newProvider = new Provider(name);
                    if (_providerController.SaveProvider(newProvider)) {
                        Log.Information($"New provider {newProvider.Designation} and {newProvider.Guid}");
                        return Ok(JsonConvert.SerializeObject(newProvider.Guid));
                    }
                    else {
                        Log.Error("AddProvider failed");
                        return BadRequest("Provider konnte nicht eingefügt werden. Bitte Admin benachrichten.");
                    }
                }
                else {
                    Log.Warning("Nothing to add");
                    return NoContent();
                }
            }
        }

        [HttpPut, Route("update")]
        public IActionResult ChangeProvider([FromHeader] Guid? guid, [FromHeader] string? designation) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("ChangeProvider called");
                if (guid is not null) {
                    Provider? result = _providerController.ExistProvider(guid ?? new Guid("00000000-0000-0000-0000-000000000000"), null);
                    if (result is not null) {
                        string oldDesignation = result.Designation;
                        result.Designation = designation ?? "";
                        if (_providerController.SaveChangesAtProvider(result)) {
                            Log.Information($"Provider successfully updated from {oldDesignation} to {designation}");
                            return Ok();
                        }
                        else {
                            Log.Error("UpdateProvider failed");
                            return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                        }
                    }
                    else {
                        Log.Warning("Guid has no entry (provider)");
                        return NotFound("Provider konnte nicht gefunden werden");
                    }
                }
                else {
                    Log.Warning("No Guid there");
                    return NoContent();
                }
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult RemoveProvider([FromHeader] Guid? guid) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("RemoveProvider called");
                if (guid is not null) {
                    Provider? result = _providerController.ExistProvider(guid ?? new Guid("00000000-0000-0000-0000"), null);
                    if (result is not null) {
                        _providerController.RemoveProvider(result);
                        Log.Information($"{result.Designation} with {result.Guid} successfully removed");
                        return Ok(JsonConvert.SerializeObject(result));
                    }
                    else {
                        Log.Warning($"{guid} not found");
                        return NotFound("Provider konnte nicht gefunden werden!");
                    }
                }
                else {
                    Log.Warning("Nothing to remove");
                    return NoContent();
                }
            }
        }
    }
}

