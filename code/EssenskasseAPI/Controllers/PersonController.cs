using EssenskasseAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/person")]
    public class PersonController : ControllerBase {

        #region Config
        private readonly Db _db;

        public PersonController(IConfiguration configuration) {
            _db = new Db(configuration);
        }
        #endregion

        #region Controller
        [HttpGet, Route("requestAll")]
        public IActionResult GetAllPerson() {
            using (Operation.Time("Request all Persons")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person " +
                    $"WHERE Sichtbar=true;";

                DataTable dataTablePerson = _db.Query(queryString);
                List<Person> persons = DataTableToListOfPerson(dataTablePerson);

                if (persons is not null) {
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
            using (Operation.Time("GetPerGuidPerson")) {
                Log.Information("GetPerGuidPerson called");
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person " +
                    $"WHERE Guid=@guid;";

                DataTable dataTablePerson = _db.Query(queryString, Db.P("@guid", guid));
                List<Person> persons = DataTableToListOfPerson(dataTablePerson);

                if (persons.Count == 1) {
                    Log.Information($"{persons[0].Name} with {persons[0].Guid} successfully requested");
                    return Ok(JsonConvert.SerializeObject(persons[0]));
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
            using (Operation.Time("AddNewPerson")) {
                Log.Information("AddNewPerson called");
                if (name is null) {
                    if (email is null) {
                        Log.Error($"Nothing to add - Header not complete {name} | {email}");
                        return BadRequest("Header unvollständig!");
                    }
                    Log.Error($"Header name is null");
                    return BadRequest("name null");
                }
                Person newPerson = new Person(
                    Uri.UnescapeDataString(name),
                    Uri.UnescapeDataString(email ?? "")
                    );
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.Person (Guid, Name, Email, SaldoEuro, SaldoCent) " +
                    $"VALUES (@guid, @name, @email, @saldoEuro, @saldoCent);";
                _db.Query(queryStringInsert,
                    Db.P("@guid", newPerson.Guid),
                    Db.P("@name", newPerson.Name),
                    Db.P("@email", newPerson.Email),
                    Db.P("@saldoEuro", newPerson.Saldo.Euro),
                    Db.P("@saldoCent", newPerson.Saldo.Cent));
                return Ok(JsonConvert.SerializeObject(newPerson.Guid));
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePerson([FromHeader] Guid guid,
                                            [FromHeader] string? name,
                                            [FromHeader] string? email) {
            using (Operation.Time("UpdatePerson")) {
                Log.Information("UpdatePerson called");

                string queryStringRequestPerson = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person " +
                    $"WHERE Guid=@guid;";
                DataTable dataTablePersonToUpdate = _db.Query(queryStringRequestPerson, Db.P("@guid", guid));

                if (dataTablePersonToUpdate.Rows.Count == 1) {
                    Person personToUpdate = DataTableToListOfPerson(dataTablePersonToUpdate)[0];
                    if (name is not null) {
                        Log.Information($"{personToUpdate.Name} to {name} change");
                        personToUpdate.Name = Uri.UnescapeDataString(name);
                    }
                    if (email is not null) {
                        Log.Information($"{personToUpdate.Email} to {email} change");
                        personToUpdate.Email = Uri.UnescapeDataString(email);
                    }

                    // DataTableToListOfPerson liefert Name/Email URL-kodiert zurueck. Wurde ein Feld
                    // nicht geaendert, muss es vor dem Speichern wieder dekodiert werden.
                    string queryStringPersonUpdate = $"" +
                        $"UPDATE essenskasse.Person " +
                        $"SET Name=@name, Email=@email " +
                        $"WHERE Guid=@guid;";
                    _db.Query(queryStringPersonUpdate,
                        Db.P("@name", name is not null ? personToUpdate.Name : Uri.UnescapeDataString(personToUpdate.Name)),
                        Db.P("@email", email is not null ? personToUpdate.Email : Uri.UnescapeDataString(personToUpdate.Email)),
                        Db.P("@guid", personToUpdate.Guid));

                    Log.Information("Person successfully updated");
                    return Ok();
                }
                else {
                    Log.Error($"UpdatePerson failed, requested Guid = '{guid}'");
                    return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                }
            }
        }

        [HttpPut, Route("updateDatabasePersonSaldo")]
        public IActionResult UpdatePersonSaldoInDatabase() {
            using (Operation.Time("UpdatePersonSaldoInDatabase")) {
                Log.Information("UpdatePersonSaldoInDatabase called");
                // get all Persons
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person;";

                DataTable dataTablePerson = _db.Query(queryString);
                List<Person> persons = DataTableToListOfPerson(dataTablePerson);

                foreach (Person person in persons) {
                    // get all Payments of Person
                    string queryStringPayments = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE ReferencedPerson=@person;";

                    DataTable dataTablePayments = _db.Query(queryStringPayments, Db.P("@person", person.Guid));
                    List<Payment> payments = DataTableToListOfPayments(dataTablePayments);

                    Money paymentsAmount = new Money(0, 0);
                    foreach (Payment x in payments) {
                        paymentsAmount = new Money().CalculateMoney(CalculationOperation.Addition, paymentsAmount, x.Amount)!;
                    }

                    // get all Orders of Person
                    string queryStringOrders = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Order " +
                        $"WHERE ReferencedPerson=@person;";

                    DataTable dataTableOrders= _db.Query(queryStringOrders, Db.P("@person", person.Guid));
                    List<Order> orders = DataTableToListOfOrders(dataTableOrders);

                    Money ordersAmount = new Money(0, 0);
                    foreach (Order x in orders) {
                        ordersAmount = new Money().CalculateMoney(CalculationOperation.Addition, ordersAmount, x.Money)!;
                    }

                    // recalculate saldo and update Person
                    Money newSaldo = new Money().CalculateMoney(CalculationOperation.Substraction, paymentsAmount, ordersAmount)!;
                    string queryStringPersonUpdate = $"" +
                        $"UPDATE essenskasse.Person " +
                        $"SET SaldoEuro=@euro, SaldoCent=@cent " +
                        $"WHERE Guid=@guid;";
                    _db.Query(queryStringPersonUpdate,
                        Db.P("@euro", newSaldo.Euro),
                        Db.P("@cent", newSaldo.Cent),
                        Db.P("@guid", person.Guid));

                    Log.Information($"{person.Guid} saldo old {person.Saldo} to new {newSaldo}");
                }
                Log.Information("Successfully recalculate Saldo");
                return Ok("Keine Fehler.");
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult RemovePerson([FromHeader] Guid? guid, bool per = false) {
            using (Operation.Time("RemovePerson")) {
                Log.Information("RemovePerson called");
                if (guid is not null) {
                    string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Person " +
                        $"WHERE Guid=@guid;";
                    DataTable dataTable = _db.Query(queryString, Db.P("@guid", guid));
                    List<Person> persons = DataTableToListOfPerson(dataTable);

                    if (persons.Count == 1) {
                        if (per == false) {
                            string queryStringInvisible = $"" +
                                $"UPDATE essenskasse.Person " +
                                $"SET Sichtbar=false " +
                                $"WHERE Guid=@guid;";
                            _db.Query(queryStringInvisible, Db.P("@guid", guid));
                            Log.Information($"{persons[0].Name} with {persons[0].Guid} successfully invisible set");
                            return Ok(JsonConvert.SerializeObject(persons[0]));
                        }
                        else {
                            string queryStringDelete = $"" +
                                $"DELETE " +
                                $"FROM essenskasse.Person " +
                                $"WHERE Guid=@guid;";
                            _db.Query(queryStringDelete, Db.P("@guid", guid));
                            Log.Information($"{persons[0].Name} with {persons[0].Guid} successfully removed");
                            return Ok(JsonConvert.SerializeObject(persons[0]));
                        }
                    }
                    Log.Warning($"{guid} not found");
                    return NotFound("Person konnte nicht gefunden werden!");
                }
                else {
                    Log.Warning("Nothing to remove");
                    return NoContent();
                }
            }
        }
        #endregion

        #region InternalFunctions
        private static List<Person> DataTableToListOfPerson(DataTable dataTable) {
            List<Person> personList = [];
            foreach (DataRow row in dataTable.Rows) {
                personList.Add(new Person(
                    guid: new Guid(row["Guid"].ToString()!),
                    name: Uri.EscapeDataString(row["Name"].ToString()!),
                    email: Uri.EscapeDataString(row["Email"].ToString()!),
                    money: new Money(
                        euro: long.Parse(row["SaldoEuro"].ToString()!),
                        cent: short.Parse(row["SaldoCent"].ToString()!)
                        )
                    ));
            }
            return personList;
        }

        private static List<Payment> DataTableToListOfPayments(DataTable dataTable) {
            List<Payment> paymentList = [];
            foreach (DataRow row in dataTable.Rows) {
                paymentList.Add(new Payment(
                    dateTime: new DateTime(long.Parse(row["DateTime"].ToString()!)),
                    amount: new Money(
                        euro: long.Parse(row["AmountEuro"].ToString()!),
                        cent: short.Parse(row["AmountCent"].ToString()!)
                        ),
                    referencedPerson: new Guid(row["ReferencedPerson"].ToString()!),
                    comment: row["Comment"].ToString()!
                    ));
            }
            return paymentList;
        }

        private static List<Order> DataTableToListOfOrders(DataTable dataTableOrders) {
            List<Order> ordersList = [];
            foreach (DataRow row in dataTableOrders.Rows) {
                ordersList.Add(new Order(
                    dateTime: new DateTime(long.Parse(row["DateTime"].ToString()!)),
                    money: new Money(
                        euro: long.Parse(row["AmountEuro"].ToString()!),
                        cent: short.Parse(row["AmountCent"].ToString()!)
                        ),
                    provider: new Provider(
                        Guid: new Guid(row["ProviderGuid"].ToString()!),
                        designation: row["ProviderDesignation"].ToString()!
                        ),
                    referencedPerson: new Guid(row["ReferencedPerson"].ToString()!)
                    ));
            }
            return ordersList;
        }
        #endregion
    }
}
