using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/payment")]
    public class PaymentController : ControllerBase {

        readonly PaymentDbController _paymentController = new PaymentDbController();

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllPayments() {
            List<Payment>? payments = _paymentController.RequestAllPayments();
            if (payments is not null) {
                return Ok(JsonConvert.SerializeObject(payments));
            }
            else {
                return NoContent();
            }
        }

        [HttpGet, Route("requestAmount")]
        public IActionResult GetAmountOfPayments([FromHeader] int? amount) {
            if (amount is null) {
                return BadRequest();
            }
            List<Payment>? payments = _paymentController.RequestAmountOfPayments(amount ?? 1);
            if (payments is not null) {
                return Ok(JsonConvert.SerializeObject(payments));
            }
            else {
                return NoContent();
            }
        }

        [HttpGet, Route("fromPerson")]
        public IActionResult GetPaymentsFromPersonPerGuid([FromHeader] Guid? guid,
                                                            [FromHeader] int? amount) {
            if (guid is null) {
                return BadRequest();
            }
            List<Payment>? payments = _paymentController.PaymentsFromOnePersonPerGuid(guid ?? new Guid(), amount);
            if (payments is not null) {
                return Ok(JsonConvert.SerializeObject(payments));
            }
            else {
                return NoContent();
            }
        }

        [HttpGet, Route("singlePerDate")]
        public IActionResult GetPaymentPerDate([FromHeader] string? dateString) {
            if (dateString is null) {
                return BadRequest();
            }
            DateTime date = DateTime.Parse(dateString);
            Payment? payment = _paymentController.PaymentAtDateTime(date);
            if (payment is not null) {
                return Ok(JsonConvert.SerializeObject(payment));
            }
            else {
                return NoContent();
            }
        }

        [HttpPost, Route("insertNew")]
        public IActionResult AddNewPayment([FromBody] Money? amount,
                                            [FromHeader] Guid? referencedPerson) {
            if ((amount is not null) && (referencedPerson is not null)) {
                Payment newPayment = new Payment(amount, referencedPerson ?? new Guid());
                if (_paymentController.NewPayment(newPayment)) {
                    return Ok(JsonConvert.SerializeObject(newPayment));
                }
                else {
                    return Conflict(JsonConvert.SerializeObject(newPayment));
                }
            }
            return BadRequest("Header unvollständig!");
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePayment([FromHeader] string? dateString,
                                            [FromHeader] Guid? referencedPerson,
                                            [FromBody] Money? money) {
            if (dateString is null) {
                return BadRequest("Kein Datum angegeben!");
            }
            DateTime date = DateTime.Parse(dateString);
            Payment? paymentToUpdate = _paymentController.PaymentAtDateTime(date);
            if (paymentToUpdate is not null) {
                if (money is not null) {
                    paymentToUpdate.Amount = money;
                }
                if (referencedPerson is not null) {
                    paymentToUpdate.ReferencedPerson = referencedPerson ?? new Guid();
                }
                if (_paymentController.UpdatePayment(paymentToUpdate)) {
                    return Ok();
                }
                else {
                    return BadRequest("Keine Einzahlung zum angegebenen Datum gefunden.");
                }
            }
            else {
                return NoContent();
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult DeletePayment([FromHeader] string? dateString) {
            if (dateString is not null) {
                DateTime date = DateTime.Parse(dateString);
                Payment? result = _paymentController.PaymentAtDateTime(date);
                if (result is not null) {
                    _paymentController.RemovePayment(result);
                    return Ok(JsonConvert.SerializeObject(result));
                }
                else {
                    return BadRequest("Einzahlung konnte nicht gefunden werden!\nFehler in der Eingabe?");
                }
            }
            else {
                return NoContent();
            }
        }
    }
}
