using EssenskasseAPI.Data;
using EssenskasseAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/person")]
    [RequireSession]
    public class PersonController : ControllerBase {

        #region Config
        private readonly Db _db;

        public PersonController(IConfiguration configuration) {
            _db = new Db(configuration);
        }
        #endregion

        #region Controller
        // Das Dashboard der PWA zeigt die Personen mit Saldo ohne Anmeldung an.
        [AllowAnonymous, HttpGet, Route("requestAll")]
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

                // Ein leerer Header (E-Mail soll geleert werden) wird vom Model Binding zu null; deshalb
                // wird hier direkt geprueft, ob der Header mitgeschickt wurde.
                if (Request.Headers.TryGetValue("email", out Microsoft.Extensions.Primitives.StringValues sentEmail)) {
                    email = sentEmail.ToString();
                }

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

        /// <summary>Ausgeblendete Personen (Sichtbar=false), damit sie wiederhergestellt werden koennen.</summary>
        [HttpGet, Route("requestHidden")]
        public IActionResult GetHiddenPerson() {
            using (Operation.Time("Request hidden Persons")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person " +
                    $"WHERE Sichtbar=false;";

                DataTable dataTablePerson = _db.Query(queryString);
                List<Person> persons = DataTableToListOfPerson(dataTablePerson);
                Log.Information(persons.Count + " hidden Person requested");
                return Ok(JsonConvert.SerializeObject(persons));
            }
        }

        /// <summary>Blendet eine ausgeblendete Person wieder ein.</summary>
        [HttpPut, Route("restore")]
        public IActionResult RestorePerson([FromHeader] Guid? guid) {
            using (Operation.Time("RestorePerson")) {
                Log.Information("RestorePerson called");
                if (guid is null) {
                    Log.Warning("Nothing to restore");
                    return BadRequest();
                }
                DataTable dataTable = _db.Query($"SELECT * FROM essenskasse.Person WHERE Guid=@guid;", Db.P("@guid", guid));
                List<Person> persons = DataTableToListOfPerson(dataTable);
                if (persons.Count != 1) {
                    Log.Warning($"{guid} not found");
                    return NotFound("Person konnte nicht gefunden werden!");
                }
                _db.Query($"UPDATE essenskasse.Person SET Sichtbar=true WHERE Guid=@guid;", Db.P("@guid", guid));
                Log.Information($"{persons[0].Name} with {persons[0].Guid} successfully visible set");
                return Ok(JsonConvert.SerializeObject(persons[0]));
            }
        }

        [HttpPut, Route("updateDatabasePersonSaldo")]
        public IActionResult UpdatePersonSaldoInDatabase() {
            using (Operation.Time("UpdatePersonSaldoInDatabase")) {
                Log.Information("UpdatePersonSaldoInDatabase called");
                // Alles in einer Transaktion: erst alle Personen sperren (so kann waehrenddessen keine Buchung
                // dazwischenkommen), dann die Summen in der Datenbank bilden und die Salden setzen.
                using DbScope tx = _db.BeginTransaction();
                DataTable dataTablePerson = tx.Query("SELECT * FROM essenskasse.Person FOR UPDATE;");
                List<Person> persons = DataTableToListOfPerson(dataTablePerson);

                Dictionary<string, long> paymentCents = SumCentsPerPerson(tx, "essenskasse.Payment");
                Dictionary<string, long> orderCents = SumCentsPerPerson(tx, "essenskasse.Order");

                foreach (Person person in persons) {
                    string key = person.Guid.ToString();
                    long paid = paymentCents.GetValueOrDefault(key, 0);
                    long ordered = orderCents.GetValueOrDefault(key, 0);

                    // recalculate saldo and update Person
                    Money newSaldo = Money.FromTotalCents(paid - ordered);
                    string queryStringPersonUpdate = $"" +
                        $"UPDATE essenskasse.Person " +
                        $"SET SaldoEuro=@euro, SaldoCent=@cent " +
                        $"WHERE Guid=@guid;";
                    tx.Query(queryStringPersonUpdate,
                        Db.P("@euro", newSaldo.Euro),
                        Db.P("@cent", newSaldo.Cent),
                        Db.P("@guid", person.Guid));

                    Log.Information($"{person.Guid} saldo old {person.Saldo} to new {newSaldo}");
                }
                tx.Commit();
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
                            try {
                                _db.Query(queryStringDelete, Db.P("@guid", guid));
                            }
                            catch (MySqlException ex) when (ex.Number == 1451) { // Fremdschluessel: es gibt noch Buchungen
                                Log.Warning($"{persons[0].Guid} has Payments/Orders and can not be removed permanently");
                                return Conflict("Die Person hat Einzahlungen oder Bestellungen und kann nicht endgültig gelöscht werden. Bitte stattdessen ausblenden (per=false).");
                            }
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
        /// <summary>Summe aller Betraege (in Cent) je Person aus Payment oder Order, gebildet in der Datenbank.</summary>
        /// <param name="tx">Laufende Transaktion.</param>
        /// <param name="table">Nur 'essenskasse.Payment' oder 'essenskasse.Order' (fester Wert, keine Eingabe).</param>
        private static Dictionary<string, long> SumCentsPerPerson(DbScope tx, string table) {
            DataTable sums = tx.Query($"SELECT ReferencedPerson, SUM(AmountEuro * 100 + AmountCent) AS TotalCents FROM {table} GROUP BY ReferencedPerson;");
            Dictionary<string, long> result = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in sums.Rows) {
                result[row["ReferencedPerson"].ToString()!] = Convert.ToInt64(row["TotalCents"]);
            }
            return result;
        }

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
