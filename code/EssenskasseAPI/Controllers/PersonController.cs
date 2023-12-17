using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Http;
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
            List<Person>? personen = _personController.RequestAllPerson();

            if (personen is not null) {
                return Ok(JsonConvert.SerializeObject(personen));
            }
            else {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("new")]
        public IActionResult NeuerPerson([FromHeader] string? name, [FromHeader] string? email) {
            if (name is not null) {
                Person newPerson = new Person(name, email);
                if (_personController.SavePerson(newPerson)) {
                    return Ok(newPerson.Guid);
                }
                else {
                    return BadRequest("Person konnte nicht eingefügt werden. Bitte Admin benachrichten.");
                }
            }
            else {
                return NoContent();
            }
        }

    }
}
