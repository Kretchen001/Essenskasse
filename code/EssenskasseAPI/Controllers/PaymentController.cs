using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/payment")]
    public class PaymentController : ControllerBase {

        #region Config
        private readonly IConfiguration Configuration;

        public PaymentController(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllPayments() {
            using (Operation.Time("Request all Payments")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Payment;";

                DataTable dataTablePayments = QueryExecuterAtMySql(queryString);
                List<Payment> paymentList = DataTableToListOfPayments(dataTablePayments);

                if (paymentList is not null) {
                    Log.Information(paymentList.Count + " Payments requested");
                    return Ok(JsonConvert.SerializeObject(paymentList));
                }
                else {
                    Log.Warning("No Data response (GetAllPayments)");
                    return NoContent();
                }
                #region oldcode
                //List<Payment>? payments = _paymentController.RequestAllPayments();
                //if (payments is not null) {
                //    Log.Information(payments.Count + " Payments requested");
                //    return Ok(JsonConvert.SerializeObject(payments));
                //}
                //else {
                //    Log.Warning("No Data response (GetAllPayments)");
                //    return NoContent();
                //}
                #endregion
            }
        }

        [HttpGet, Route("requestAmount")]
        public IActionResult GetAmountOfPayments([FromHeader] int? amount) {
            using (Operation.Time("Request amount of Payments")) {
                if (amount is null) {
                    Log.Warning("GetAmountOfPayments with no Header called");
                    return BadRequest();
                }
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Payment " +
                    $"ORDER BY DateTime DESC " +
                    $"LIMIT {amount};";

                DataTable dataTablePayments = QueryExecuterAtMySql(queryString);
                List<Payment> payments = DataTableToListOfPayments(dataTablePayments);

                if (payments.Count != 0) {
                    Log.Information(payments.Count + " Payments requested");
                    return Ok(JsonConvert.SerializeObject(payments));
                }
                else {
                    Log.Warning("No Data response (GetAmountOfPayments)");
                    return NoContent();
                }
                #region oldcode
                //if (amount is null) {
                //    Log.Warning("GetAmountOfPayments with no Header called");
                //    return BadRequest();
                //}
                //List<Payment> payments = _paymentController.RequestAmountOfPayments(amount ?? 1);
                //if (payments.Count != 0) {
                //    Log.Information(payments.Count + " Payments requested");
                //    return Ok(JsonConvert.SerializeObject(payments));
                //}
                //else {
                //    Log.Warning("No Data response (GetAmountOfPayments)");
                //    return NoContent();
                //}
                #endregion
            }
        }

        [HttpGet, Route("fromPerson")]
        public IActionResult GetPaymentsFromPersonPerGuid([FromHeader] Guid? guid,
                                                            [FromHeader] int? amount) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetPaymentsFromPersonPerGuid called");
                if (guid is null) {
                    Log.Warning("Guid Header empty");
                    return BadRequest();
                }
                string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE essenkasse.Payment='{guid}' " +
                        $"ORDER BY DateTime DESC;";
                if (amount is null) { // request all
                    queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE essenkasse.Payment='{guid}' " +
                        $"ORDER BY DateTime DESC " +
                        $"LIMIT {amount};";
                }
                DataTable dataTableOrders = QueryExecuterAtMySql(queryString);
                List<Payment> payments = DataTableToListOfPayments(dataTableOrders);
                if (payments.Count != 0) {
                    Log.Information(guid.ToString() + " requested and " + payments.Count + " found.");
                    return Ok(JsonConvert.SerializeObject(payments));
                }
                else {
                    Log.Warning(guid.ToString() + " no equivalent entry found.");
                    return NoContent();
                }
                #region oldcode
                //Log.Information("GetPaymentsFromPersonPerGuid called");
                //if (guid is null) {
                //    Log.Warning("Guid Header empty");
                //    return BadRequest();
                //}
                //List<Payment> payments = _paymentController.PaymentsFromOnePersonPerGuid(guid ?? new Guid(), amount);
                //if (payments.Count != 0) {
                //    Log.Information(guid.ToString() + " requested and " + payments.Count + " found.");
                //    return Ok(JsonConvert.SerializeObject(payments));
                //}
                //else {
                //    Log.Warning(guid.ToString() + " no equivalent entry found.");
                //    return NoContent();
                //}
                #endregion
            }
        }

        [HttpGet, Route("singlePerDate")]
        public IActionResult GetPaymentPerDate([FromHeader] string? dateString) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetCountOfOrdersFromDay called");
                if (dateString is not null) {
                    DateTime date = DateTime.Parse(dateString);
                    string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment;";

                    DataTable dataTablePayments = QueryExecuterAtMySql(queryString);
                    List<Payment> payments = DataTableToListOfPayments(dataTablePayments);
                    List<Payment> removeCandidates = [];
                    foreach (Payment payment in payments) {
                        if (date.Date != payment.DateTime.Date) {
                            removeCandidates.Add(payment);
                        }
                    }
                    payments.RemoveAll(x => removeCandidates.Contains(x));
                    string temp = date.ToString("yyyy-MM-ddTHH:mm:ss");
                    Log.Information($"{payments.Count} Payments at {temp} found and responsed");
                    if (payments.Count == 1) {
                        return Ok(JsonConvert.SerializeObject(payments[0]));
                    }
                    else {
                        return Ok(JsonConvert.SerializeObject(payments));
                    }
                }
                else {
                    Log.Warning("Empty dateString Header");
                    return BadRequest("Leeres Datum");
                }
                #region oldcode
                //Log.Information("GetPaymentPerDate called");
                //if (dateString is null) {
                //    Log.Warning("dateString Header empty");
                //    return BadRequest();
                //}
                //DateTime date = DateTime.Parse(dateString);
                //Payment? payment = _paymentController.PaymentAtDateTime(date);
                //if (payment is not null) {
                //    Log.Information($"Payment on {dateString} successfully requested");
                //    return Ok(JsonConvert.SerializeObject(payment));
                //}
                //else {
                //    Log.Warning(dateString.ToString() + " no equivalent entry found.");
                //    return NoContent();
                //}
                #endregion
            }
        }

        [HttpPost, Route("insertNew")]
        public IActionResult AddNewPayment([FromBody] Money? amount,
                                            [FromHeader] Guid? referencedPerson,
                                            [FromHeader] string? comment) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddNewPayment called");
                if ((amount is not null) && (referencedPerson is not null)) {
                    Payment newPayment = new Payment(amount, referencedPerson ?? new Guid(), comment ?? "");
                    switch (FullInsertProcessForOnePayment(newPayment)) {
                        case 0: {
                                return Ok(JsonConvert.SerializeObject(newPayment));
                            }
                        case 1: {
                                return NotFound("Provider existiert nicht!");
                            }
                        case 2: {
                                return NotFound("Person existiert nicht!");
                            }
                        default: {
                                return BadRequest();
                            }
                    }
                }
                Log.Error($"Nothing to add - Header not complete {amount} | {referencedPerson}");
                return BadRequest("Header unvollständig!");
                #region oldcode
                //Log.Information("AddNewPayment called");
                //if ((amount is not null) && (referencedPerson is not null)) {
                //    Payment newPayment = new Payment(amount, referencedPerson ?? new Guid(), comment ?? "");
                //    if (_paymentController.NewPayment(newPayment)) {
                //        // recalculate Person Saldo
                //        PersonDbController _personDbController = new PersonDbController();
                //        Person? x = _personDbController.PersonPerGuid(referencedPerson ?? new Guid());
                //        if (x is not null) {
                //            Log.Information($"Person {x.Guid} {x.Name} old Saldo = {x.Saldo}");
                //            x.Saldo = new Money().CalculateMoney(CalculationOperation.Addition, x.Saldo, newPayment.Amount)!;
                //            Log.Information($"Person {x.Guid} {x.Name} new Saldo = {x.Saldo}");
                //            _personDbController.UpdatePerson(x);
                //        }
                //        Log.Information($"New Payment of {amount} for Person added.");
                //        return Ok(JsonConvert.SerializeObject(newPayment));
                //    }
                //    else {
                //        Log.Warning("New Payment not inserted. Conflict return");
                //        return Conflict(JsonConvert.SerializeObject(newPayment));
                //    }
                //}
                //Log.Error($"Nothing to add - Header not complete {amount} | {referencedPerson}");
                //return BadRequest("Header unvollständig!");
                #endregion
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePayment([FromHeader] long? dateAsLong,
                                            [FromHeader] Guid? referencedPerson,
                                            [FromHeader] string? comment,
                                            [FromBody] Money? money) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("UpdatePayment called");
                if (dateAsLong is null) {
                    Log.Warning("No dateString in Header");
                    return BadRequest("Kein Datum angegeben!");
                }
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Payment " +
                    $"WHERE DateTime={dateAsLong}";
                DataTable dataTable = QueryExecuterAtMySql(queryString);
                List<Payment> payments = DataTableToListOfPayments(dataTable);

                if (payments.Count == 0) {
                    Log.Warning($"At {dateAsLong} no Payment found");
                    return NotFound("Keine Einzahlung zum angegebenen Datum gefunden.");
                }

                bool hasChanges = false;
                if (referencedPerson is not null) {
                    Log.Information($"Payment updated from {payments[0].ReferencedPerson} to {referencedPerson}");
                    payments[0].ReferencedPerson = referencedPerson ?? new Guid();
                    hasChanges = true;
                }
                if (money is not null) {
                    Log.Information($"Payment updated from {payments[0].Amount} to {money}");
                    payments[0].Amount = money;
                    hasChanges = true;
                }
                if (comment is not null) {
                    Log.Information($"Payment updated from {payments[0].Comment} to {comment}");
                    payments[0].Comment = comment ?? "";
                    hasChanges = true;
                }

                if (hasChanges) {
                    // update person saldo
                    string queryStringUpdateSaldo = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Person " +
                        $"WHERE Guid='{payments[0].ReferencedPerson}';";
                    DataTable dataTablePersonToUpdate = QueryExecuterAtMySql(queryStringUpdateSaldo);

                    if (dataTablePersonToUpdate.Rows.Count == 1) {
                        Person personToUpdate = DataTableToPerson(dataTablePersonToUpdate);
                        Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} old Saldo = {personToUpdate.Saldo}");
                        personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Substraction, personToUpdate.Saldo, payments[0].Amount)!;
                        Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} new Saldo = {personToUpdate.Saldo}");

                        string queryStringPersonUpdate = $"" +
                            $"UPDATE essenskasse.Person " +
                            $"SET SaldoEuro = {personToUpdate.Saldo.Euro}, SaldoCent = {personToUpdate.Saldo.Cent} " +
                            $"WHERE Guid='{personToUpdate.Guid}';";
                        QueryExecuterAtMySql(queryStringPersonUpdate);
                    }

                    string queryStringUpdate = $"" +
                        $"UPDATE essenskasse.Payment " +
                        $"SET AmountEuro={payments[0].Amount.Euro}, AmountCent={payments[0].Amount.Cent}, ReferencedPerson={payments[0].ReferencedPerson}, Comment='{comment}' " +
                        $"WHERE DateTime='{dateAsLong}';";
                    QueryExecuterAtMySql(queryStringUpdate);
                    Log.Information("Payment successfully updated");
                    return Ok();
                }
                Log.Warning($"At {dateAsLong} nothing to change at payment.");
                return NotFound("Keine Änderung an der Einzahlung zum angegebenen Datum.");
                #region oldcode
                //Log.Information("UpdatePayment called");
                //if (dateString is null) {
                //    Log.Warning("No dateString in Header");
                //    return BadRequest("Kein Datum angegeben!");
                //}
                //DateTime date = DateTime.Parse(dateString);
                //Payment? paymentToUpdate = _paymentController.PaymentAtDateTime(date);
                //if (paymentToUpdate is not null) {
                //    if (money is not null) {
                //        Log.Information($"Money of Payment updated from {paymentToUpdate.Amount} to {money}");
                //        paymentToUpdate.Amount = money;
                //    }
                //    if (referencedPerson is not null) {
                //        Log.Information($"RefrencedPerson of Payment updated from {paymentToUpdate.ReferencedPerson} to {referencedPerson}");
                //        paymentToUpdate.ReferencedPerson = referencedPerson ?? new Guid();
                //    }
                //    if (_paymentController.UpdatePayment(paymentToUpdate)) {
                //        // recalculate Person Saldo
                //        PersonDbController _personDbController = new PersonDbController();
                //        Person? x = _personDbController.PersonPerGuid(referencedPerson ?? new Guid());
                //        if (x is not null) {
                //            Log.Information($"Person {x.Guid} {x.Name} old Saldo = {x.Saldo}");
                //            x.Saldo = new Money().CalculateMoney(CalculationOperation.Addition, x.Saldo, paymentToUpdate.Amount)!;
                //            Log.Information($"Person {x.Guid} {x.Name} new Saldo = {x.Saldo}");
                //            _personDbController.UpdatePerson(x);
                //        }
                //        Log.Information($"Payment successfully updated");
                //        return Ok();
                //    }
                //    else {
                //        Log.Warning($"At {dateString} no payment found");
                //        return NotFound("Keine Einzahlung zum angegebenen Datum gefunden.");
                //    }
                //}
                //else {
                //    Log.Warning("No payment");
                //    return BadRequest();
                //}
                #endregion
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult DeletePayment([FromHeader] long? dateAsLong) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("DeleteOrder called");
                if (dateAsLong is not null) {
                    string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Payment " +
                    $"WHERE DateTime={dateAsLong}";
                    DataTable dataTable = QueryExecuterAtMySql(queryString);
                    List<Payment> payments = DataTableToListOfPayments(dataTable);
                    if (payments.Count == 0) {
                        Log.Warning($"At {dateAsLong} no Payment found");
                        return NotFound("Einzahlung konnte nicht gefunden werden!\nFehler in der Eingabe?");
                    }
                    string queryStringDelete = $"" +
                        $"DELETE " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE DateTime={dateAsLong};";
                    QueryExecuterAtMySql(queryStringDelete);
                    Log.Information($"Payment at {payments[0].DateTime.Ticks} successfully removed");
                    return Ok(JsonConvert.SerializeObject(payments[0]));
                }
                else {
                    Log.Warning("Empty dateAsLong Header");
                    return BadRequest();
                }
                #region oldcode
                //Log.Information("DeletePayment called");
                //if (dateString is not null) {
                //    DateTime date = DateTime.Parse(dateString);
                //    Payment? result = _paymentController.PaymentAtDateTime(date);
                //    if (result is not null) {
                //        _paymentController.RemovePayment(result);
                //        Log.Information($"Payment at {result.DateTime} successfully removed");
                //        return Ok(JsonConvert.SerializeObject(result));
                //    }
                //    else {
                //        Log.Warning($"At {dateString} no payment found");
                //        return NotFound("Einzahlung konnte nicht gefunden werden!\nFehler in der Eingabe?");
                //    }
                //}
                //else {
                //    Log.Warning("Empty dateString Header");
                //    return BadRequest();
                //}
                #endregion
            }
        }

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
        private Person DataTableToPerson(DataTable DataTablePersonToUpdate) {
            return new Person(
                guid: new Guid(DataTablePersonToUpdate.Rows[0]["Guid"].ToString()!),
                name: DataTablePersonToUpdate.Rows[0]["Name"].ToString()!,
                email: DataTablePersonToUpdate.Rows[0]["Email"].ToString()!,
                money: new Money(
                    euro: long.Parse(DataTablePersonToUpdate.Rows[0]["SaldoEuro"].ToString()!),
                    cent: short.Parse(DataTablePersonToUpdate.Rows[0]["SaldoCent"].ToString()!)
                    )
                );
        }

        private int FullInsertProcessForOnePayment(Payment payment) {
            // provider exist?
            string queryString = $"" +
                $"SELECT * " +
                $"FROM essenskasse.Person " +
                $"WHERE Guid='{payment.ReferencedPerson};";

            DataTable dataTableProvider = QueryExecuterAtMySql(queryString);
            if (dataTableProvider.Rows.Count != 1) {
                Log.Warning($"{payment.ReferencedPerson} not available...");
                return 1;
            }

            // create and insert new order
            string queryStringInsert = $"" +
                $"INSERT INTO essenskasse.Payment (DateTime, AmountEuro, AmountCent, ReferencedPerson, Comment) " +
                $"VALUES (" +
                $"{payment.DateTime.Ticks}, " +
                $"{payment.Amount.Euro}, " +
                $"{payment.Amount.Cent}, " +
                $"'{payment.ReferencedPerson}', " +
                $"'{payment.Comment}');";
            QueryExecuterAtMySql(queryStringInsert);

            // update person saldo
            string queryStringUpdateSaldo = $"" +
                $"SELECT * " +
                $"FROM essenskasse.Person " +
                $"WHERE Guid='{payment.ReferencedPerson}';";
            DataTable dataTablePersonToUpdate = QueryExecuterAtMySql(queryStringUpdateSaldo);

            if (dataTablePersonToUpdate.Rows.Count == 1) {
                Person personToUpdate = DataTableToPerson(dataTablePersonToUpdate);
                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} old Saldo = {personToUpdate.Saldo}");
                personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Substraction, personToUpdate.Saldo, payment.Money)!;
                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} new Saldo = {personToUpdate.Saldo}");

                string queryStringPersonUpdate = $"" +
                    $"UPDATE essenskasse.Person " +
                    $"SET SaldoEuro = {personToUpdate.Saldo.Euro}, SaldoCent = {personToUpdate.Saldo.Cent} " +
                    $"WHERE Guid='{personToUpdate.Guid}';";
                QueryExecuterAtMySql(queryStringPersonUpdate);

                Log.Information($"New Payment {payment.Amount} for Person added.");
                return 0;
            }
            return 2;
        }
        #endregion
    }
}
