using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;

namespace EssenskasseAPI.Controllers {

    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase {

        PaymentDbController _paymentController = new PaymentDbController();

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
        public IActionResult GetPaymentsFromPersonPerGuid([FromHeader] Guid? guid) {
            if (guid is null) {
                return BadRequest();
            }
            List<Payment>? payments = _paymentController.PaymentsFromOnePersonPerGuid(guid ?? new Guid());
            if (payments is not null) {
                return Ok(JsonConvert.SerializeObject(payments));
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
                    return Ok(newPayment);
                }
                else {
                    return Conflict(newPayment);
                }
            }
            return BadRequest("Header unvollständig!");
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePayment([FromHeader] string? dateString,
                                            [FromBody] Money? money,
                                            [FromHeader] Guid? referencedPerson) {
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
                    return BadRequest();
                }
            }
            else {
                return NoContent();
            }
        }
    }
}
