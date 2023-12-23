using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/order")]
    public class OrderController : ControllerBase {

        readonly ProviderDbController _providerController = new ProviderDbController();
        readonly OrderDbController _orderDbController = new OrderDbController();

        [HttpGet, Route("fromPerson")]
        public IActionResult GetPaymentsFromPersonPerGuid([FromHeader] Guid? guid) {
            if (guid is null) {
                return BadRequest();
            }
            List<Payment>? payments = _orderDbController.OrdersFromOnePersonPerGuid(guid ?? new Guid());
            if (payments is not null) {
                return Ok(JsonConvert.SerializeObject(payments));
            }
            else {
                return NoContent();
            }
        }

        [HttpPost, Route("insertNew")]
        public IActionResult AddNewPayment([FromBody] Order? order) {
            if (order is not null) {
                if (_providerController.ExistProvider(
                                            guid: order.Provider.Guid,
                                            name: order.Provider.Designation) is null) {
                    return BadRequest("Provider existiert nicht!");
                }
                if (_orderDbController.NewOrder(order)) {
                    return Ok(JsonConvert.SerializeObject(order));
                }
                else {
                    return Conflict(JsonConvert.SerializeObject(order));
                }
            }
            return BadRequest("Body unvollständig!");
        }
    }
}
