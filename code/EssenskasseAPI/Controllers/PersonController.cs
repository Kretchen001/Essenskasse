using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;

namespace EssenskasseAPI.Controllers {

    [Route("api/person")]
    [ApiController]
    public class PersonController : Controller {

        PersonDbController _personController = new PersonDbController();

        [HttpGet]
        [Route("requestAll")]
        public IActionResult GetAllPerson() {
            List<Person>? persons = _personController.RequestAllPerson();

            if (persons is not null) {
                foreach (Person x in persons) {
                    x.Name = Uri.EscapeDataString(x.Name);
                    x.Email = Uri.EscapeDataString(x.Email ?? "");
                }
                return Ok(JsonConvert.SerializeObject(persons));
            }
            else {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("perGuid")]
        public IActionResult GetPerGuidPerson([FromHeader] Guid guid) {
            Person? person = _personController.PersonPerGuid(guid);
            if (person is not null) {
                person.Name = Uri.EscapeDataString(person.Name);
                person.Email = Uri.EscapeDataString(person.Email ?? "");
                return Ok(JsonConvert.SerializeObject(person));
            }
            else {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("new")]
        public IActionResult AddNewPerson([FromHeader] string? name,
                                            [FromHeader] string? email) {
            if (name is not null) { 
                Person newPerson = new Person(
                    Uri.UnescapeDataString(name),
                    Uri.UnescapeDataString(email ?? ""));
                if (_personController.SavePerson(newPerson)) {
                    return Ok(JsonConvert.SerializeObject(newPerson.Guid));
                }
                else {
                    return BadRequest("Person konnte nicht eingefügt werden. Bitte Admin benachrichten.");
                }
            }
            else {
                return NoContent();
            }
        }

        [HttpPut]
        [Route("update")]
        public IActionResult UpdatePerson([FromHeader] Guid personGuid,
                                            [FromHeader] string? name,
                                            [FromHeader] string? email) {
            Person? personToUpdate = _personController.PersonPerGuid(personGuid);
            if (personToUpdate is not null) {
                if (name is not null) {
                    personToUpdate.Name = name;
                }
                if (email is not null) {
                    personToUpdate.Email = email;
                }

                if (_personController.UpdatePerson(personToUpdate)) {
                    return Ok();
                }
                else {
                    return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                }
            }
            else {
                return NoContent();
            }
        }

        [HttpDelete]
        [Route("remove")]
        public IActionResult RemovePerson([FromHeader] Guid? guid) {
            if (guid is not null) {
                Person? result = _personController.ExistPerson(guid ?? null, null);
                if (result is not null) {
                    _personController.RemovePerson(result);
                    return Ok(JsonConvert.SerializeObject(result));
                }
                else {
                    return BadRequest("Person konnte nicht gefunden werden!");
                }
            }
            else {
                return NoContent();
            }
        }
    }
}
