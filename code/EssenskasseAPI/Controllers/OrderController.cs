using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/order")]
    public class OrderController : ControllerBase {

        readonly ProviderDbController _providerController = new ProviderDbController();
        readonly OrderDbController _orderDbController = new OrderDbController();

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
