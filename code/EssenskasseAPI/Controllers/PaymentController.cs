using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;

namespace EssenskasseAPI.Controllers {

    [Route("api/payment")]
    [ApiController]
    public class PaymentController : Controller{

        PaymentDbController _paymentController = new PaymentDbController();

        [HttpPost, Route("insertNew")]
        public IActionResult AddNewPayment([FromHeader] Money? amount,
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
    }
}
