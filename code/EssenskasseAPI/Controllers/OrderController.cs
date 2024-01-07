using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/order")]
    public class OrderController : ControllerBase {

        readonly ProviderDbController _providerController = new ProviderDbController();
        readonly OrderDbController _orderDbController = new OrderDbController();

        [HttpGet, Route("requestAll")]
        public IActionResult GetAllOrders() {
            using (Operation.Time("TaskDuration")) {
                List<Order>? orders = _orderDbController.RequestAllOrders();
                if (orders is not null) {
                    Log.Information(orders.Count + " Orders requested");
                    return Ok(JsonConvert.SerializeObject(orders));
                }
                else {
                    Log.Warning("No Data response (GetAllOrders)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("requestAmount")]
        public IActionResult GetAmountOfOrders([FromHeader] int? amount) {
            using (Operation.Time("TaskDuration")) {
                if (amount is null) {
                    Log.Warning("GetAmountOfOrders with no Header called");
                    return BadRequest();
                }
                List<Order> orders = _orderDbController.RequestAmountOfOrders(amount ?? 1);
                if (orders.Count != 0) {
                    Log.Information(orders.Count + " Orders requested");
                    return Ok(JsonConvert.SerializeObject(orders));
                }
                else {
                    Log.Warning("No Data response (GetAllOrders)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("fromPerson")]
        public IActionResult GetOrdersFromPersonPerGuid([FromHeader] Guid? guid,
                                                            [FromHeader] int? amount) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetOrdersFromPersonPerGuid called");
                if (guid is null) {
                    Log.Warning("Guid Header empty");
                    return BadRequest();
                }
                List<Order> orders = new List<Order>();
                if (amount is null) { // request all
                    orders = _orderDbController.OrdersFromOnePersonPerGuid(guid ?? new Guid());
                }
                else { // request amount
                    orders = _orderDbController.OrdersFromOnePersonPerGuid(guid ?? new Guid(), amount);
                }
                if (orders.Count != 0) {
                    Log.Information(guid.ToString() + " requested and " + orders.Count + " found.");
                    return Ok(JsonConvert.SerializeObject(orders));
                }
                else {
                    Log.Warning(guid.ToString() + " no equivalent entry found.");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("requestDate")]
        public IActionResult GetCountOfOrdersFromDay([FromHeader] string? dateString) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetCountOfOrdersFromDay called");
                if (dateString is not null) {
                    DateTime date = DateTime.Parse(dateString);
                    List<Order> orders = _orderDbController.OrdersAtDay(date);
                    Log.Information($"{orders.Count} Orders at {dateString} found and responsed");
                    return Ok(JsonConvert.SerializeObject(orders.Count));
                }
                else {
                    Log.Warning("Empty dateString Header");
                    return BadRequest("Leeres Datum");
                }
            }
        }

        [HttpPost, Route("insertNew")]
        public IActionResult AddNewOrder([FromBody] Order? order) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddNewOrder called");
                if (order is not null) {
                    if (_providerController.ExistProvider(
                                            guid: order.Provider.Guid,
                                            name: order.Provider.Designation) is null) {
                        Log.Warning($"{order.Provider.Guid} not available...");
                        return NotFound("Provider existiert nicht!");
                    }
                    if (_orderDbController.NewOrder(order)) {
                        // recalculate Person Saldo
                        PersonDbController _personDbController = new PersonDbController();
                        Person? x = _personDbController.PersonPerGuid(order.ReferencedPerson);
                        if (x is not null) {
                            Log.Information($"Person {x.Guid} {x.Name} old Saldo = {x.Saldo}");
                            x.Saldo = new Money().CalculateMoney(CalculationOperation.Substraction, x.Saldo, order.Money)!;
                            Log.Information($"Person {x.Guid} {x.Name} new Saldo = {x.Saldo}");
                            _personDbController.UpdatePerson(x);
                        }
                        Log.Information($"New Order of {order.Money.ToString()} for Person added.");
                        return Ok(JsonConvert.SerializeObject(order));
                    }
                    else {
                        Log.Warning("New Order not inserted. Conflict return");
                        return Conflict(JsonConvert.SerializeObject(order));
                    }
                }
                Log.Warning("Empty order Header");
                return BadRequest("Body unvollständig!");
            }
        }

        [HttpPost, Route("insertNewList")]
        public IActionResult AddNewOrderList([FromBody] List<Order>? orders) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddNewOrderList called");
                if (orders is not null) {
                    if (_providerController.ExistProvider(
                                                guid: orders[0].Provider.Guid,
                                                name: orders[0].Provider.Designation) is null) {
                        Log.Warning($"{orders[0].Provider.Guid} not available...");
                        return NotFound("Provider existiert nicht!");
                    }
                    bool success = true;
                    foreach (Order x in orders) {
                        if (!_orderDbController.NewOrder(x)) {
                            Log.Warning($"Order at{x.DateTime} with {x.ReferencedPerson} from Provider {x.Provider.Guid} not inserted");
                            success = false;
                            break;
                        }
                        else {
                            // recalculate Person Saldo
                            PersonDbController _personDbController = new PersonDbController();
                            Person? y = _personDbController.PersonPerGuid(x.ReferencedPerson);
                            if (y is not null) {
                                Log.Information($"Person {y.Guid} {y.Name} old Saldo = {x.Money}");
                                y.Saldo = new Money().CalculateMoney(
                                    method: CalculationOperation.Substraction,
                                    money1: y.Saldo,
                                    money2: x.Money
                                )!;
                                Log.Information($"Person {y.Guid} {y.Name} new Saldo = {x.Money}");
                                _personDbController.UpdatePerson(y);
                            }
                        }
                    }
                    if (success) {
                        Log.Information("All Orders successfully added");
                        return Ok();
                    }
                    else {
                        Log.Warning("New Order not inserted. Conflict return");
                        return Conflict();
                    }
                }
                Log.Warning("Empty orders Header");
                return BadRequest("Body unvollständig!");
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdatePayment([FromHeader] string? dateString,
                                            [FromBody] Tuple<Money?, Provider?, Guid?> values) {
            using (Operation.Time("TaskDuration")) {
                if (dateString is null) {
                    Log.Warning("Empty dateString Header");
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
                        // recalculate Person Saldo
                        PersonDbController _personDbController = new PersonDbController();
                        Person? x = _personDbController.PersonPerGuid(orderToUpdate.ReferencedPerson);
                        if (x is not null) {
                            x.Saldo = new Money().CalculateMoney(CalculationOperation.Substraction, x.Saldo, orderToUpdate.Money)!;
                            _personDbController.UpdatePerson(x);
                        }
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

        [HttpDelete, Route("remove")]
        public IActionResult DeletePayment([FromHeader] string? dateString) {
            using (Operation.Time("TaskDuration")) {
                if (dateString is not null) {
                    DateTime date = DateTime.Parse(dateString);
                    Order? result = _orderDbController.OrderAtDateTime(date);
                    if (result is not null) {
                        _orderDbController.RemoveOrder(result);
                        return Ok(JsonConvert.SerializeObject(result));
                    }
                    else {
                        return BadRequest("Bestelung konnte nicht gefunden werden!\nFehler in der Eingabe?");
                    }
                }
                else {
                    return NoContent();
                }
            }
        }
    }
}
