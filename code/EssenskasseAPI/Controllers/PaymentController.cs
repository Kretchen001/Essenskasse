using EssenskasseAPI.Data;
using EssenskasseAPI.Security;
using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/payment")]
    [RequireSession]
    public class PaymentController : ControllerBase {

        #region Config
        private readonly Db _db;

        public PaymentController(IConfiguration configuration) {
            _db = new Db(configuration);
        }
        #endregion

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllPayments() {
            using (Operation.Time("Request all Payments")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Payment;";

                DataTable dataTablePayments = _db.Query(queryString);
                List<Payment> paymentList = DataTableToListOfPayments(dataTablePayments);

                if (paymentList is not null) {
                    Log.Information(paymentList.Count + " Payments requested");
                    return Ok(JsonConvert.SerializeObject(paymentList));
                }
                else {
                    Log.Warning("No Data response (GetAllPayments)");
                    return NoContent();
                }
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
                    $"LIMIT @amount;";

                DataTable dataTablePayments = _db.Query(queryString, Db.P("@amount", amount));
                List<Payment> payments = DataTableToListOfPayments(dataTablePayments);

                if (payments.Count != 0) {
                    Log.Information(payments.Count + " Payments requested");
                    return Ok(JsonConvert.SerializeObject(payments));
                }
                else {
                    Log.Warning("No Data response (GetAmountOfPayments)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("requestNextAmount")]
        public IActionResult GetNextAmountOfPayments([FromHeader] int? amount,
                                                                [FromHeader] int? alreadyLoaded) {
            using (Operation.Time("Request next amount of Payments")) {
                Log.Information("GetNextAmountOfPayments called");
                if (amount is null) {
                    Log.Warning("GetNextAmountOfPayments with empty amount Header called");
                    return BadRequest();
                }
                if (alreadyLoaded is null) {
                    Log.Warning("GetAmountOfPayments with empty alreadyLoaded Header called");
                    return BadRequest();
                }
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Payment " +
                    $"ORDER BY DateTime DESC " +
                    $"LIMIT @amount OFFSET @alreadyLoaded;";

                DataTable dataTablePayments = _db.Query(queryString,
                    Db.P("@amount", amount),
                    Db.P("@alreadyLoaded", alreadyLoaded));
                List<Payment> payments = DataTableToListOfPayments(dataTablePayments);

                if (payments.Count != 0) {
                    Log.Information(payments.Count + " next Payments requested");
                    return Ok(JsonConvert.SerializeObject(payments));
                }
                else {
                    Log.Warning("No Data response (GetAmountOfPayments)");
                    return NoContent();
                }
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
                        $"WHERE ReferencedPerson=@guid " +
                        $"ORDER BY DateTime DESC;";
                if (amount is not null) { // request amount
                    queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE ReferencedPerson=@guid " +
                        $"ORDER BY DateTime DESC " +
                        $"LIMIT @amount;";
                }
                DataTable dataTableOrders = _db.Query(queryString,
                    Db.P("@guid", guid),
                    Db.P("@amount", amount));
                List<Payment> payments = DataTableToListOfPayments(dataTableOrders);
                if (payments.Count != 0) {
                    Log.Information(guid.ToString() + " requested and " + payments.Count + " found.");
                    return Ok(JsonConvert.SerializeObject(payments));
                }
                else {
                    Log.Warning(guid.ToString() + " no equivalent entry found.");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("requestNextAmountFromPerson")]
        public IActionResult GetNextAmountOfPaymentsFromPerson([FromHeader] Guid? guid,
                                                                [FromHeader] int? amount,
                                                                [FromHeader] int? alreadyLoaded) {
            using (Operation.Time("Request next amount of Payments")) {
                Log.Information("GetNextAmountOfPaymentsFromPerson called");
                if (guid is null) {
                    Log.Warning("Guid Header empty");
                    return BadRequest();
                }
                if (amount is null) {
                    Log.Warning("GetNextAmountOfPaymentsFromPerson with empty amount Header called");
                    return BadRequest();
                }
                if (alreadyLoaded is null) {
                    Log.Warning("GetNextAmountOfPaymentsFromPerson with empty alreadyLoaded Header called");
                    return BadRequest();
                }
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Payment " +
                    $"WHERE ReferencedPerson=@guid " +
                    $"ORDER BY DateTime DESC " +
                    $"LIMIT @amount OFFSET @alreadyLoaded;";

                DataTable dataTablePayments = _db.Query(queryString,
                    Db.P("@guid", guid),
                    Db.P("@amount", amount),
                    Db.P("@alreadyLoaded", alreadyLoaded));
                List<Payment> payments = DataTableToListOfPayments(dataTablePayments);

                if (payments.Count != 0) {
                    Log.Information(payments.Count + " next Payments requested");
                    return Ok(JsonConvert.SerializeObject(payments));
                }
                else {
                    Log.Warning("No Data response (GetAmountOfPayments)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("singlePerDate")]
        public IActionResult GetPaymentPerDate([FromHeader] string? dateString) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetCountOfOrdersFromDay called");
                if (dateString is not null) {
                    DateTime date = DateTime.Parse(dateString);
                    // Die Datenbank speichert Ticks: alle Einzahlungen des Tages liegen in [Tagesbeginn, Tagesbeginn + 1 Tag).
                    string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE DateTime >= @dayStart AND DateTime < @dayEnd;";

                    DataTable dataTablePayments = _db.Query(queryString,
                        Db.P("@dayStart", date.Date.Ticks),
                        Db.P("@dayEnd", date.Date.AddDays(1).Ticks));
                    List<Payment> payments = DataTableToListOfPayments(dataTablePayments);
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
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePayment([FromHeader] Guid? accessToken,
                                            [FromHeader] long? dateAsLong,
                                            [FromHeader] Guid? referencedPerson,
                                            [FromHeader] string? comment,
                                            [FromBody] Money? money) {
            using (Operation.Time("UpdatePayment")) {
                Log.Information("UpdatePayment called");
                if (accessToken is not null) {
                    string queryStringToken = $"" +
                            $"SELECT COUNT(*) " +
                            $"FROM essenskasse.User " +
                            $"WHERE TokenForChange=@accessToken;";
                    DataTable dataTableTokenCount = _db.Query(queryStringToken, Db.P("@accessToken", accessToken));
                    // COUNT(*) liefert immer genau eine Zeile; entscheidend ist der Zaehlerwert.
                    // Die Null-Guid ist der Standardwert "kein Token" und darf nie gueltig sein.
                    if (accessToken != Guid.Empty && Convert.ToInt64(dataTableTokenCount.Rows[0][0]) > 0) {
                        Log.Information($"{accessToken} called UpdatePayment.");
                        if (dateAsLong is null) {
                            Log.Warning("No dateString in Header");
                            return BadRequest("Kein Datum angegeben!");
                        }
                        string queryString = $"" +
                            $"SELECT * " +
                            $"FROM essenskasse.Payment " +
                            $"WHERE DateTime=@dateAsLong;";
                        DataTable dataTable = _db.Query(queryString, Db.P("@dateAsLong", dateAsLong));
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
                        Money temp = payments[0].Amount;
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
                                $"WHERE Guid=@guid;";
                            DataTable dataTablePersonToUpdate = _db.Query(queryStringUpdateSaldo, Db.P("@guid", payments[0].ReferencedPerson));

                            if (dataTablePersonToUpdate.Rows.Count == 1) {
                                Person personToUpdate = DataTableToPerson(dataTablePersonToUpdate);
                                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} old Saldo = {personToUpdate.Saldo}");
                                personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Substraction, personToUpdate.Saldo, temp)!;
                                personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Addition, personToUpdate.Saldo, payments[0].Amount)!;
                                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} new Saldo = {personToUpdate.Saldo}");

                                string queryStringPersonUpdate = $"" +
                                    $"UPDATE essenskasse.Person " +
                                    $"SET SaldoEuro=@euro, SaldoCent=@cent " +
                                    $"WHERE Guid=@guid;";
                                _db.Query(queryStringPersonUpdate,
                                    Db.P("@euro", personToUpdate.Saldo.Euro),
                                    Db.P("@cent", personToUpdate.Saldo.Cent),
                                    Db.P("@guid", personToUpdate.Guid));
                            }

                            // payments[0].Comment ist der geaenderte Kommentar oder (ohne Aenderung) der bisherige.
                            string queryStringUpdate = $"" +
                                $"UPDATE essenskasse.Payment " +
                                $"SET AmountEuro=@euro, AmountCent=@cent, ReferencedPerson=@person, Comment=@comment " +
                                $"WHERE DateTime=@dateAsLong;";
                            _db.Query(queryStringUpdate,
                                Db.P("@euro", payments[0].Amount.Euro),
                                Db.P("@cent", payments[0].Amount.Cent),
                                Db.P("@person", payments[0].ReferencedPerson),
                                Db.P("@comment", payments[0].Comment),
                                Db.P("@dateAsLong", dateAsLong));
                            Log.Information("Payment successfully updated");
                            return Ok();
                        }
                        Log.Warning($"At {dateAsLong} nothing to change at payment.");
                        return NotFound("Keine Änderung an der Einzahlung zum angegebenen Datum.");
                    }
                }
                Log.Warning("No AccessToken in Request");
                return BadRequest("No AccessToken in Request");
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
                        $"WHERE DateTime=@dateAsLong;";
                    DataTable dataTable = _db.Query(queryString, Db.P("@dateAsLong", dateAsLong));
                    List<Payment> payments = DataTableToListOfPayments(dataTable);
                    if (payments.Count == 0) {
                        Log.Warning($"At {dateAsLong} no Payment found");
                        return NotFound("Einzahlung konnte nicht gefunden werden!\nFehler in der Eingabe?");
                    }
                    string queryStringDelete = $"" +
                        $"DELETE " +
                        $"FROM essenskasse.Payment " +
                        $"WHERE DateTime=@dateAsLong;";
                    _db.Query(queryStringDelete, Db.P("@dateAsLong", dateAsLong));
                    Log.Information($"Payment at {payments[0].DateTime.Ticks} successfully removed");
                    return Ok(JsonConvert.SerializeObject(payments[0]));
                }
                else {
                    Log.Warning("Empty dateAsLong Header");
                    return BadRequest();
                }
            }
        }

        #region InternalFunctions
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

        private static Person DataTableToPerson(DataTable DataTablePersonToUpdate) {
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
            // person exist?
            string queryString = $"" +
                $"SELECT * " +
                $"FROM essenskasse.Person " +
                $"WHERE Guid=@guid;";

            DataTable dataTablePerson = _db.Query(queryString, Db.P("@guid", payment.ReferencedPerson));
            if (dataTablePerson.Rows.Count != 1) {
                Log.Warning($"{payment.ReferencedPerson} not available...");
                return 1;
            }

            // create and insert new payment
            string queryStringInsert = $"" +
                $"INSERT INTO essenskasse.Payment (DateTime, AmountEuro, AmountCent, ReferencedPerson, Comment) " +
                $"VALUES (@dateTime, @euro, @cent, @person, @comment);";
            _db.Query(queryStringInsert,
                Db.P("@dateTime", payment.DateTime.Ticks),
                Db.P("@euro", payment.Amount.Euro),
                Db.P("@cent", payment.Amount.Cent),
                Db.P("@person", payment.ReferencedPerson),
                Db.P("@comment", payment.Comment));

            // update person saldo
            string queryStringUpdateSaldo = $"" +
                $"SELECT * " +
                $"FROM essenskasse.Person " +
                $"WHERE Guid=@guid;";
            DataTable dataTablePersonToUpdate = _db.Query(queryStringUpdateSaldo, Db.P("@guid", payment.ReferencedPerson));

            if (dataTablePersonToUpdate.Rows.Count == 1) {
                Person personToUpdate = DataTableToPerson(dataTablePersonToUpdate);
                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} old Saldo = {personToUpdate.Saldo}");
                personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Addition, personToUpdate.Saldo, payment.Amount)!;
                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} new Saldo = {personToUpdate.Saldo}");

                string queryStringPersonUpdate = $"" +
                    $"UPDATE essenskasse.Person " +
                    $"SET SaldoEuro=@euro, SaldoCent=@cent " +
                    $"WHERE Guid=@guid;";
                _db.Query(queryStringPersonUpdate,
                    Db.P("@euro", personToUpdate.Saldo.Euro),
                    Db.P("@cent", personToUpdate.Saldo.Cent),
                    Db.P("@guid", personToUpdate.Guid));

                Log.Information($"New Payment {payment.Amount} for Person added.");
                return 0;
            }
            return 2;
        }
        #endregion
    }
}
