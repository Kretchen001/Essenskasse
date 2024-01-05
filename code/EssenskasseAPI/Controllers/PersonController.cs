using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/person")]
    public class PersonController : ControllerBase {

        readonly PersonDbController _personController = new PersonDbController();

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllPerson() {
            using (Operation.Time("TaskDuration")) {
                List<Person> persons = _personController.RequestAllPerson();

                if (persons.Count != 0) {
                    foreach (Person x in persons) {
                        x.Name = Uri.EscapeDataString(x.Name);
                        x.Email = Uri.EscapeDataString(x.Email ?? "");
                    }
                    Log.Information(persons.Count + " Person requested");
                    return Ok(JsonConvert.SerializeObject(persons));
                }
                else {
                    Log.Warning("No Data response (GetAllPerson)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("perGuid")]
        public IActionResult GetPerGuidPerson([FromHeader] Guid guid) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetPerGuidPerson called");
                Person? person = _personController.PersonPerGuid(guid);
                if (person is not null) {
                    person.Name = Uri.EscapeDataString(person.Name);
                    person.Email = Uri.EscapeDataString(person.Email ?? "");
                    Log.Information($"{person.Name} with {person.Guid} successfully requested");
                    return Ok(JsonConvert.SerializeObject(person));
                }
                else {
                    Log.Warning(guid.ToString() + " requested, but no entry found.");
                    return NotFound();
                }
            }
        }

        [HttpPost, Route("new")]
        public IActionResult AddNewPerson([FromHeader] string? name,
                                            [FromHeader] string? email) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddNewPerson called");
                if (name is not null) {
                    Person newPerson = new Person(
                        Uri.UnescapeDataString(name),
                        Uri.UnescapeDataString(email ?? ""));
                    if (_personController.SavePerson(newPerson)) {
                        Log.Information($"New person {newPerson.Name} & {newPerson.Email} and {newPerson.Guid}");
                        return Ok(JsonConvert.SerializeObject(newPerson.Guid));
                    }
                    else {
                        Log.Error("AddNewPerson failed");
                        return BadRequest("Person konnte nicht eingefügt werden. Bitte Admin benachrichten.");
                    }
                }
                else {
                    Log.Warning("Nothing to add");
                    return NoContent();
                }
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePerson([FromHeader] Guid guid,
                                            [FromHeader] string? name,
                                            [FromHeader] string? email) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("UpdatePerson called");
                Person? personToUpdate = _personController.PersonPerGuid(guid);
                if (personToUpdate is not null) {
                    if (name is not null) {
                        Log.Information($"{personToUpdate.Name} to {name} change");
                        personToUpdate.Name = Uri.UnescapeDataString(name);
                    }
                    if (email is not null) {
                        Log.Information($"{personToUpdate.Email} to {email} change");
                        personToUpdate.Email = Uri.UnescapeDataString(email);
                    }

                    if (_personController.UpdatePerson(personToUpdate)) {
                        Log.Information("Person successfully updated");
                        return Ok();
                    }
                    else {
                        Log.Error("UpdatePerson failed");
                        return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                    }
                }
                else {
                    Log.Warning("Guid has no entry (person)");
                    return NoContent();
                }
            }
        }

        [HttpPut, Route("updateDatabasePersonSaldo")]
        public IActionResult UpdatePersonSaldoInDatabase() {
            using (Operation.Time("TaskDuration")) {
                Log.Information("UpdatePersonSaldoInDatabase called");
                SaldoDbController _saldoDbController = new SaldoDbController();
                try {
                    _saldoDbController.RecalculateSaldoInTheDatabase();
                    Log.Information("Successfully recalculate Saldo");
                    return Ok("Keine Fehler.");
                }
                catch (Exception ex) {
                    Log.Error($"Saldorecalculation throw {ex.Message}");
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult RemovePerson([FromHeader] Guid? guid) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("RemovePerson called");
                if (guid is not null) {
                    Person? result = _personController.ExistPerson(guid ?? null, null);
                    if (result is not null) {
                        _personController.RemovePerson(result);
                        Log.Information($"{result.Name} with {result.Guid} successfully removed");
                        return Ok(JsonConvert.SerializeObject(result));
                    }
                    else {
                        Log.Warning($"{guid} not found");
                        return NotFound("Person konnte nicht gefunden werden!");
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
