using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/order")]
    public class OrderController : ControllerBase {

        readonly ProviderDbController _providerController = new ProviderDbController();
        readonly OrderDbController _orderDbController = new OrderDbController();

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllOrders() {
            List<Order>? orders = _orderDbController.RequestAllOrders();
            if (orders is not null) {
                return Ok(JsonConvert.SerializeObject(orders));
            }
            else {
                return NoContent();
            }
        }

        [HttpGet, Route("requestAmount")]
        public IActionResult GetAmountOfOrders([FromHeader] int? amount) {
            if (amount is null) {
                return BadRequest();
            }
            List<Order>? orders = _orderDbController.RequestAmountOfOrders(amount ?? 1);
            if (orders is not null) {
                return Ok(JsonConvert.SerializeObject(orders));
            }
            else {
                return NoContent();
            }
        }

        [HttpGet, Route("fromPerson")]
        public IActionResult GetOrdersFromPersonPerGuid([FromHeader] Guid? guid) {
            if (guid is null) {
                return BadRequest();
            }
            List<Order>? orders = _orderDbController.OrdersFromOnePersonPerGuid(guid ?? new Guid());
            if (orders is not null) {
                return Ok(JsonConvert.SerializeObject(orders));
            }
            else {
                return NoContent();
            }
        }

        [HttpPost, Route("insertNew")]
        public IActionResult AddNewOrder([FromBody] Order? order) {
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


        [HttpPut, Route("update")]
        public IActionResult UpdatePayment([FromHeader] string? dateString,
                                            [FromBody] Tuple<Money?, Provider?, Guid?> values) {
            if (dateString is null) {
                return BadRequest("Kein Datum angegeben!");
            }
            DateTime date = DateTime.Parse(dateString);
            Order? orderToUpdate = _orderDbController.OrderAtDateTime(date);
            if (orderToUpdate is not null) {
                if (values.Item1 is not null) {
                    orderToUpdate.Money = values.Item1;
                }
                if (values.Item2 is not null) {
                    orderToUpdate.Provider = values.Item2;
                }
                if (values.Item3 is not null) {
                    orderToUpdate.ReferencedPerson = values.Item3 ?? new Guid();
                }
                if (_orderDbController.UpdateOrder(orderToUpdate)) {
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
    }
}
