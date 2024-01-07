using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/payment")]
    public class PaymentController : ControllerBase {

        readonly PaymentDbController _paymentController = new PaymentDbController();

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllPayments() {
            using (Operation.Time("TaskDuration")) {
                List<Payment>? payments = _paymentController.RequestAllPayments();
                if (payments is not null) {
                    Log.Information(payments.Count + " Payments requested");
                    return Ok(JsonConvert.SerializeObject(payments));
                }
                else {
                    Log.Warning("No Data response (GetAllPayments)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("requestAmount")]
        public IActionResult GetAmountOfPayments([FromHeader] int? amount) {
            using (Operation.Time("TaskDuration")) {
                if (amount is null) {
                    Log.Warning("GetAmountOfPayments with no Header called");
                    return BadRequest();
                }
                List<Payment> payments = _paymentController.RequestAmountOfPayments(amount ?? 1);
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

        [HttpGet, Route("fromPerson")]
        public IActionResult GetPaymentsFromPersonPerGuid([FromHeader] Guid? guid,
                                                            [FromHeader] int? amount) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetPaymentsFromPersonPerGuid called");
                if (guid is null) {
                    Log.Warning("Guid Header empty");
                    return BadRequest();
                }
                List<Payment> payments = _paymentController.PaymentsFromOnePersonPerGuid(guid ?? new Guid(), amount);
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

        [HttpGet, Route("singlePerDate")]
        public IActionResult GetPaymentPerDate([FromHeader] string? dateString) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetPaymentPerDate called");
                if (dateString is null) {
                    Log.Warning("dateString Header empty");
                    return BadRequest();
                }
                DateTime date = DateTime.Parse(dateString);
                Payment? payment = _paymentController.PaymentAtDateTime(date);
                if (payment is not null) {
                    Log.Information($"Payment on {dateString} successfully requested");
                    return Ok(JsonConvert.SerializeObject(payment));
                }
                else {
                    Log.Warning(dateString.ToString() + " no equivalent entry found.");
                    return NoContent();
                }
            }
        }

        [HttpPost, Route("insertNew")]
        public IActionResult AddNewPayment([FromBody] Money? amount,
                                            [FromHeader] Guid? referencedPerson) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddNewPayment called");
                if ((amount is not null) && (referencedPerson is not null)) {
                    Payment newPayment = new Payment(amount, referencedPerson ?? new Guid());
                    if (_paymentController.NewPayment(newPayment)) {
                        // recalculate Person Saldo
                        PersonDbController _personDbController = new PersonDbController();
                        Person? x = _personDbController.PersonPerGuid(referencedPerson ?? new Guid());
                        if (x is not null) {
                            Log.Information($"Person {x.Guid} {x.Name} old Saldo = {x.Saldo}");
                            x.Saldo = new Money().CalculateMoney(CalculationOperation.Addition, x.Saldo, newPayment.Amount)!;
                            Log.Information($"Person {x.Guid} {x.Name} new Saldo = {x.Saldo}");
                            _personDbController.UpdatePerson(x);
                        }
                        Log.Information($"New Payment of {amount.ToString()} for Person added.");
                        return Ok(JsonConvert.SerializeObject(newPayment));
                    }
                    else {
                        Log.Warning("New Payment not inserted. Conflict return");
                        return Conflict(JsonConvert.SerializeObject(newPayment));
                    }
                }
                Log.Error($"Nothing to add - Header not complete {amount} | {referencedPerson}");
                return BadRequest("Header unvollständig!");
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePayment([FromHeader] string? dateString,
                                            [FromHeader] Guid? referencedPerson,
                                            [FromBody] Money? money) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("UpdatePayment called");
                if (dateString is null) {
                    Log.Warning("No dateString in Header");
                    return BadRequest("Kein Datum angegeben!");
                }
                DateTime date = DateTime.Parse(dateString);
                Payment? paymentToUpdate = _paymentController.PaymentAtDateTime(date);
                if (paymentToUpdate is not null) {
                    if (money is not null) {
                        Log.Information($"Money of Payment updated from {paymentToUpdate.Amount.ToString()} to {money.ToString()}");
                        paymentToUpdate.Amount = money;
                    }
                    if (referencedPerson is not null) {
                        Log.Information($"RefrencedPerson of Payment updated from {paymentToUpdate.ReferencedPerson} to {referencedPerson}");
                        paymentToUpdate.ReferencedPerson = referencedPerson ?? new Guid();
                    }
                    if (_paymentController.UpdatePayment(paymentToUpdate)) {
                        // recalculate Person Saldo
                        PersonDbController _personDbController = new PersonDbController();
                        Person? x = _personDbController.PersonPerGuid(referencedPerson ?? new Guid());
                        if (x is not null) {
                            Log.Information($"Person {x.Guid} {x.Name} old Saldo = {x.Saldo}");
                            x.Saldo = new Money().CalculateMoney(CalculationOperation.Addition, x.Saldo, paymentToUpdate.Amount)!;
                            Log.Information($"Person {x.Guid} {x.Name} new Saldo = {x.Saldo}");
                            _personDbController.UpdatePerson(x);
                        }
                        Log.Information($"Payment successfully updated");
                        return Ok();
                    }
                    else {
                        Log.Warning($"At {dateString} no payment found");
                        return NotFound("Keine Einzahlung zum angegebenen Datum gefunden.");
                    }
                }
                else {
                    Log.Warning("No payment");
                    return BadRequest();
                }
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult DeletePayment([FromHeader] string? dateString) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("DeletePayment called");
                if (dateString is not null) {
                    DateTime date = DateTime.Parse(dateString);
                    Payment? result = _paymentController.PaymentAtDateTime(date);
                    if (result is not null) {
                        _paymentController.RemovePayment(result);
                        Log.Information($"Payment at {result.DateTime.ToString()} successfully removed");
                        return Ok(JsonConvert.SerializeObject(result));
                    }
                    else {
                        Log.Warning($"At {dateString} no payment found");
                        return NotFound("Einzahlung konnte nicht gefunden werden!\nFehler in der Eingabe?");
                    }
                }
                else {
                    Log.Warning("Empty dateString Header");
                    return BadRequest();
                }
            }
        }
    }
}
