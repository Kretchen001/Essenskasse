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
        private readonly IConfiguration Configuration;

        public PersonController(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        #region Controller
        [HttpGet, Route("requestAll")]
        public IActionResult GetAllPerson() {
            using (Operation.Time("Request all Payments")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person;";

                DataTable dataTablePerson = QueryExecuterAtMySql(queryString);
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
            #region oldcode
            //using (Operation.Time("TaskDuration")) {
            //    List<Person> persons = _personController.RequestAllPerson();

            //    if (persons.Count != 0) {
            //        foreach (Person x in persons) {
            //            x.Name = Uri.EscapeDataString(x.Name);
            //            x.Email = Uri.EscapeDataString(x.Email ?? "");
            //        }
            //        Log.Information(persons.Count + " Person requested");
            //        return Ok(JsonConvert.SerializeObject(persons));
            //    }
            //    else {
            //        Log.Warning("No Data response (GetAllPerson)");
            //        return NoContent();
            //    }
            //}
            #endregion
        }

        [HttpGet, Route("perGuid")]
        public IActionResult GetPerGuidPerson([FromHeader] Guid guid) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetPerGuidPerson called");
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person " +
                    $"WHERE essenskasse.Person.Guid='{guid}';";

                DataTable dataTablePerson = QueryExecuterAtMySql(queryString);
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
            #region oldcode
            //using (Operation.Time("TaskDuration")) {
            //    Log.Information("GetPerGuidPerson called");
            //    Person? person = _personController.PersonPerGuid(guid);
            //    if (person is not null) {
            //        person.Name = Uri.EscapeDataString(person.Name);
            //        person.Email = Uri.EscapeDataString(person.Email ?? "");
            //        Log.Information($"{person.Name} with {person.Guid} successfully requested");
            //        return Ok(JsonConvert.SerializeObject(person));
            //    }
            //    else {
            //        Log.Warning(guid.ToString() + " requested, but no entry found.");
            //        return NotFound();
            //    }
            //}
            #endregion
        }

        [HttpPost, Route("new")]
        public IActionResult AddNewPerson([FromHeader] string? name,
                                            [FromHeader] string? email) {
            using (Operation.Time("TaskDuration")) {
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
                    $"VALUES (" +
                    $"'{newPerson.Guid}'," +
                    $"'{newPerson.Name}', " +
                    $"'{newPerson.Email}', " +
                    $"{newPerson.Saldo.Euro}, " +
                    $"{newPerson.Saldo.Cent});";
                QueryExecuterAtMySql(queryStringInsert);
                return Ok(JsonConvert.SerializeObject(newPerson.Guid));
                #region oldcode
                //if (name is not null) {
                //    Person newPerson = new Person(
                //        Uri.UnescapeDataString(name),
                //        Uri.UnescapeDataString(email ?? ""));
                //    if (_personController.SavePerson(newPerson)) {
                //        Log.Information($"New person {newPerson.Name} & {newPerson.Email} and {newPerson.Guid}");
                //        return Ok(JsonConvert.SerializeObject(newPerson.Guid));
                //    }
                //    else {
                //        Log.Error("AddNewPerson failed");
                //        return BadRequest("Person konnte nicht eingefügt werden. Bitte Admin benachrichten.");
                //    }
                //}
                //else {
                //    Log.Warning("Nothing to add");
                //    return NoContent();
                //}
                #endregion
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePerson([FromHeader] Guid guid,
                                            [FromHeader] string? name,
                                            [FromHeader] string? email) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("UpdatePerson called");

                string queryStringRequestPerson = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person " +
                    $"WHERE Guid='{guid}';";
                DataTable dataTablePersonToUpdate = QueryExecuterAtMySql(queryStringRequestPerson);

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

                    string queryStringPersonUpdate = $"" +
                        $"UPDATE essenskasse.Person " +
                        $"SET Name='{personToUpdate.Name}, Email='{personToUpdate.Email}' " +
                        $"WHERE Guid='{personToUpdate.Guid}';";
                    QueryExecuterAtMySql(queryStringPersonUpdate);

                    Log.Information("Person successfully updated");
                    return Ok();
                    #region oldcode
                    //if (_personController.UpdatePerson(personToUpdate)) {
                    //    Log.Information("Person successfully updated");
                    //    return Ok();
                    //}
                    //else {
                    //    Log.Error("UpdatePerson failed");
                    //    return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                    //}
                    #endregion
                }
                else {
                    Log.Error($"UpdatePerson failed, requested Guid = '{guid}'");
                    return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                }
            }
        }

        [HttpPut, Route("updateDatabasePersonSaldo")]
        public IActionResult UpdatePersonSaldoInDatabase() {
            using (Operation.Time("TaskDuration")) {
                Log.Information("UpdatePersonSaldoInDatabase called");
                // get all Persons
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Person;";

                DataTable dataTablePerson = QueryExecuterAtMySql(queryString);
                List<Person> persons = DataTableToListOfPerson(dataTablePerson);

                foreach (Person person in persons) {
                    // get all Payments of Person
                    string queryStringPayments = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE ReferencedPerson='{person.Guid}';";

                    DataTable dataTablePayments = QueryExecuterAtMySql(queryStringPayments);
                    List<Payment> payments = DataTableToListOfPayments(dataTablePayments);

                    Money paymentsAmount = new Money(0, 0);
                    foreach (Payment x in payments) {
                        paymentsAmount = new Money().CalculateMoney(CalculationOperation.Addition, paymentsAmount, x.Amount)!;
                    }

                    // get all Orders of Person
                    string queryStringOrders = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE ReferencedPerson='{person.Guid}';";

                    DataTable dataTableOrders= QueryExecuterAtMySql(queryStringOrders);
                    List<Order> orders = DataTableToListOfOrders(dataTableOrders);

                    Money ordersAmount = new Money(0, 0);
                    foreach (Order x in orders) {
                        ordersAmount = new Money().CalculateMoney(CalculationOperation.Addition, ordersAmount, x.Money)!;
                    }

                    // recalculate saldo and update Person
                    Money newSaldo = new Money().CalculateMoney(CalculationOperation.Substraction, paymentsAmount, ordersAmount)!;
                    string queryStringPersonUpdate = $"" +
                        $"UPDATE essenskasse.Person " +
                        $"SET SaldoEuro={newSaldo.Euro}, SaldoCent={newSaldo.Cent} " +
                        $"WHERE Guid='{person.Guid}';";
                    QueryExecuterAtMySql(queryStringPersonUpdate);

                    Log.Information($"{person.Guid} saldo old {person.Saldo} to new {newSaldo}");
                }
                Log.Information("Successfully recalculate Saldo");
                return Ok("Keine Fehler.");
                #region oldcode
                //SaldoDbController _saldoDbController = new SaldoDbController();
                //try {
                //    _saldoDbController.RecalculateSaldoInTheDatabase();
                //    Log.Information("Successfully recalculate Saldo");
                //    return Ok("Keine Fehler.");
                //}
                //catch (Exception ex) {
                //    Log.Error($"Saldorecalculation throw {ex.Message} | {ex.StackTrace}");
                //    return BadRequest(ex.Message + "\n" + ex.StackTrace);
                //}
                #endregion
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult RemovePerson([FromHeader] Guid? guid) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("RemovePerson called");
                if (guid is not null) {
                    string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Person " +
                        $"WHERE Guid={guid}";
                    DataTable dataTable = QueryExecuterAtMySql(queryString);
                    List<Person> persons = DataTableToListOfPerson(dataTable);

                    if (persons.Count == 1) {
                        string queryStringDelete = $"" +
                            $"DELETE " +
                            $"FROM essenskasse.Person" +
                            $"WHERE Guid={guid};";
                        QueryExecuterAtMySql(queryStringDelete);
                        Log.Information($"{persons[0].Name} with {persons[0].Guid} successfully removed");
                        return Ok(JsonConvert.SerializeObject(persons[0]));
                    }
                    Log.Warning($"{guid} not found");
                    return NotFound("Person konnte nicht gefunden werden!");
                    #region oldcode
                    //Person? result = _personController.ExistPerson(guid ?? null, null);
                    //if (result is not null) {
                    //    _personController.RemovePerson(result);
                    //    Log.Information($"{result.Name} with {result.Guid} successfully removed");
                    //    return Ok(JsonConvert.SerializeObject(result));
                    //}
                    //else {
                    //    Log.Warning($"{guid} not found");
                    //    return NotFound("Person konnte nicht gefunden werden!");
                    //}
                    #endregion
                }
                else {
                    Log.Warning("Nothing to remove");
                    return NoContent();
                }
            }
        }
        #endregion

        #region InternalFunctions
        private DataTable QueryExecuterAtMySql(string Query) {
            // MySql Connection
            MySqlConnection connection = new MySqlConnection(Configuration["ConnectionStrings:essenskasse"]);

            MySqlCommand cmd = new MySqlCommand(Query, connection);

            DataTable dataTable = new DataTable();
            // read the result
            connection.Open();
            MySqlDataReader reader = cmd.ExecuteReader();
            dataTable.Load(reader);
            connection.Close();

            return dataTable;
        }

        private List<Person> DataTableToListOfPerson(DataTable dataTable) {
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

        private List<Payment> DataTableToListOfPayments(DataTable dataTable) {
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

        private List<Order> DataTableToListOfOrders(DataTable dataTableOrders) {
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
