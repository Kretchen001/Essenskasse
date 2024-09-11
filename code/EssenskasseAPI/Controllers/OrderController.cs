using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/order")]
    public class OrderController : ControllerBase {

        #region Config
        private readonly IConfiguration Configuration;

        public OrderController(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        #region Controller
        [HttpGet, Route("requestAll")]
        public IActionResult GetAllOrders() {
            using (Operation.Time("request all Orders")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Order;";

                DataTable dataTableOrders = QueryExecuterAtMySql(queryString);
                List<Order> ordersList = DataTableToListOfOrders(dataTableOrders);

                if (ordersList is not null) {
                    Log.Information(ordersList.Count + " Orders requested");
                    return Ok(JsonConvert.SerializeObject(ordersList));
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
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Order " +
                    $"ORDER BY DateTime DESC " +
                    $"LIMIT {amount};";

                DataTable dataTableOrders = QueryExecuterAtMySql(queryString);
                List<Order> orders = DataTableToListOfOrders(dataTableOrders);
                if (orders.Count != 0) {
                    Log.Information(orders.Count + " Orders requested");
                    return Ok(JsonConvert.SerializeObject(orders));
                }
                else {
                    Log.Warning("No Data response (GetAmountOfOrders)");
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("requestNextAmount")]
        public IActionResult GetNextAmountOfOrders([FromHeader] int? amount,
                                                    [FromHeader] int? alreadyLoaded) {
            using (Operation.Time("Request next amount of Payments")) {
                Log.Information("GetNextAmountOfOrders called");
                if (amount is null) {
                    Log.Warning("GetNextAmountOfOrders with empty amount Header called");
                    return BadRequest();
                }
                if (alreadyLoaded is null) {
                    Log.Warning("GetNextAmountOfOrders with empty alreadyLoaded Header called");
                    return BadRequest();
                }
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Order " +
                    $"ORDER BY DateTime DESC " +
                    $"LIMIT {amount} OFFSET {alreadyLoaded};";

                DataTable dataTableOrders = QueryExecuterAtMySql(queryString);
                List<Order> oders = DataTableToListOfOrders(dataTableOrders);

                if (oders.Count != 0) {
                    Log.Information(oders.Count + " next Order requested");
                    return Ok(JsonConvert.SerializeObject(oders));
                }
                else {
                    Log.Warning("No Data response (GetNextAmountOfOrders)");
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
                string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Order " +
                        $"WHERE ReferencedPerson='{guid}' " +
                        $"ORDER BY DateTime DESC;";
                if (amount is not null) { // request amount
                    queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Order " +
                        $"WHERE ReferencedPerson='{guid}' " +
                        $"ORDER BY DateTime DESC " +
                        $"LIMIT {amount};";
                }
                DataTable dataTableOrders = QueryExecuterAtMySql(queryString);
                List<Order> orders = DataTableToListOfOrders(dataTableOrders);
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
                    string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Order;";

                    DataTable dataTableOrders = QueryExecuterAtMySql(queryString);
                    List<Order> orders = DataTableToListOfOrders(dataTableOrders);
                    List<Order> removeCandidates = [];
                    foreach (Order order in orders) {
                        if (date.Date != order.DateTime.Date) {
                            removeCandidates.Add(order);
                        }
                    }
                    orders.RemoveAll(x => removeCandidates.Contains(x));
                    //List<Order> orders = _orderDbController.OrdersAtDay(date);
                    string temp = date.ToString("yyyy-MM-ddTHH:mm:ss");
                    Log.Information($"{orders.Count} Orders at {temp} found and responsed");
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
                    switch (FullInsertProcessForOneOrder(order)) {
                        case 0: {
                                return Ok(JsonConvert.SerializeObject(order));
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
                Log.Warning("Empty order Header");
                return BadRequest("Body unvollständig!");
            }
        }

        [HttpPost, Route("insertNewList")]
        public IActionResult AddNewOrderList([FromBody] List<Order>? orders) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddNewOrderList called");
                if (orders is not null) {
                    foreach (Order order in orders) {
                        switch (FullInsertProcessForOneOrder(order)) {
                            case 0: {
                                    continue;
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
                    Log.Information("All Orders successfully added");
                    return Ok();
                }
                Log.Warning("Empty orders Header");
                return BadRequest("Body unvollständig!");
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdateOrder([FromHeader] Guid? accessToken,
                                            [FromHeader] long? dateAsLong,
                                            [FromBody] MoneyProviderGuidData values) {
            using (Operation.Time("Update Order")) {
                Log.Information("UpdateOrder called");
                if (accessToken is not null) {
                    string queryStringToken = $"" +
                            $"SELECT COUNT(*) " +
                            $"FROM essenskasse.User " +
                            $"WHERE TokenForChange='{accessToken}';";
                    DataTable dataTableTokenCount = QueryExecuterAtMySql(queryStringToken);
                    if (dataTableTokenCount.Rows.Count == 1) {
                        Log.Information($"{accessToken} called UpdateOrder.");
                        if (dateAsLong is null) {
                            Log.Warning("Empty dateAsLong Header");
                            return BadRequest("Kein Datum angegeben!");
                        }
                        string queryString = $"" +
                            $"SELECT * " +
                            $"FROM essenskasse.Order " +
                            $"WHERE DateTime={dateAsLong};";
                        DataTable dataTable = QueryExecuterAtMySql(queryString);
                        List<Order> orders = DataTableToListOfOrders(dataTable);

                        if (orders.Count == 0) {
                            Log.Warning($"At {dateAsLong} no Order found");
                            return NotFound("Keine Bestellung zum angegebenen Datum gefunden.");
                        }

                        bool hasChanges = false;
                        Money temp = orders[0].Money;
                        if (values.Money is not null) {
                            Log.Information($"Order updated from {orders[0].Money} to {values.Money}");
                            orders[0].Money = values.Money;
                            hasChanges = true;
                        }
                        if (values.Provider is not null) {
                            Log.Information($"Order updated from {orders[0].Provider} to {values.Provider}");
                            orders[0].Provider = values.Provider;
                            hasChanges = true;
                        }
                        if (values.ReferencedPerson is not null) {
                            Log.Information($"Order updated from {orders[0].ReferencedPerson} to {values.ReferencedPerson}");
                            orders[0].ReferencedPerson = values.ReferencedPerson ?? new Guid();
                            hasChanges = true;
                        }
                        if (hasChanges) {
                            // update person saldo
                            string queryStringUpdateSaldo = $"" +
                                $"SELECT * " +
                                $"FROM essenskasse.Person " +
                                $"WHERE Guid='{orders[0].ReferencedPerson}';";
                            DataTable dataTablePersonToUpdate = QueryExecuterAtMySql(queryStringUpdateSaldo);

                            if (dataTablePersonToUpdate.Rows.Count == 1) {
                                Person personToUpdate = DataTableToPerson(dataTablePersonToUpdate);
                                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} old Saldo = {personToUpdate.Saldo}");
                                personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Addition, personToUpdate.Saldo, temp)!;
                                personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Substraction, personToUpdate.Saldo, orders[0].Money)!;
                                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} new Saldo = {personToUpdate.Saldo}");

                                string queryStringPersonUpdate = $"" +
                                    $"UPDATE essenskasse.Person " +
                                    $"SET SaldoEuro={personToUpdate.Saldo.Euro}, SaldoCent={personToUpdate.Saldo.Cent} " +
                                    $"WHERE Guid='{personToUpdate.Guid}';";
                                QueryExecuterAtMySql(queryStringPersonUpdate);
                            }

                            string queryStringUpdate = $"" +
                                $"UPDATE essenskasse.Order " +
                                $"SET AmountEuro={orders[0].Money.Euro}, AmountCent={orders[0].Money.Cent}, ProviderGuid='{orders[0].Provider.Guid}', ProviderDesignation='{orders[0].Provider.Designation}', ReferencedPerson='{orders[0].ReferencedPerson}' " +
                                $"WHERE DateTime='{dateAsLong}';";
                            QueryExecuterAtMySql(queryStringUpdate);
                            Log.Information("Order successfully updated");
                            return Ok();
                        }
                        Log.Warning($"At {dateAsLong} no Order updated. Nichts zum updaten angegeben.");
                        return NotFound("Keine Bestellung zum angegebenen Datum geupdated...\nNichts zum updaten angegeben.");
                    }
                    Log.Warning("AccessToken wrong");
                    return BadRequest("AccessToken wrong");
                }
                Log.Warning("AccessToken wrong");
                return BadRequest("AccessToken wrong");
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult DeleteOrder([FromHeader] long? dateAsLong) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("DeleteOrder called");
                if (dateAsLong is not null) {
                    string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Order " +
                    $"WHERE DateTime={dateAsLong}";
                    DataTable dataTable = QueryExecuterAtMySql(queryString);
                    List<Order> orders = DataTableToListOfOrders(dataTable);
                    if (orders.Count == 0) {
                        Log.Warning($"At {dateAsLong} no Order found");
                        return NotFound("Bestellung konnte nicht gefunden werden!\nFehler in der Eingabe?");
                    }
                    string queryStringDelete = $"" +
                        $"DELETE " +
                        $"FROM essenskasse.Order " +
                        $"WHERE DateTime={dateAsLong};";
                    QueryExecuterAtMySql(queryStringDelete);
                    Log.Information($"Order at {orders[0].DateTime.Ticks} successfully removed");
                    return Ok(JsonConvert.SerializeObject(orders[0]));
                }
                else {
                    Log.Warning("Empty dateAsLong Header");
                    return BadRequest();
                }
            }
        }
        #endregion

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

        private static List<Order> DataTableToListOfOrders(DataTable dataTableOrders) {
            List<Order> ordersList = [];
            foreach (DataRow row in dataTableOrders.Rows) {
                ordersList.Add(new Order(
                    dateTime: new DateTime(long.Parse(row["DateTime"].ToString()!)),
                    money: new Money(
                        euro: long.Parse(row["AmountEuro"].ToString()!),
                        cent: short.Parse(row["AmountCent"].ToString()!)
                        ),
                    provider: new Provider(
                        Guid: new Guid(row["ProviderGuid"].ToString()!),
                        designation: row["ProviderDesignation"].ToString()!
                        ),
                    referencedPerson: new Guid(row["ReferencedPerson"].ToString()!)
                    ));
            }
            return ordersList;
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

        private int FullInsertProcessForOneOrder(Order order) {
            // provider exist?
            string queryString = $"" +
                $"SELECT * " +
                $"FROM essenskasse.Provider " +
                $"WHERE (Guid='{order.Provider.Guid}') AND (Designation='{order.Provider.Designation}');";

            DataTable dataTableProvider = QueryExecuterAtMySql(queryString);
            if (dataTableProvider.Rows.Count != 1) {
                Log.Warning($"{order.Provider.Guid} not available...");
                return 1;
            }

            // create and insert new order
            string queryStringInsert = $"" +
                $"INSERT INTO essenskasse.Order (DateTime, AmountEuro, AmountCent, ProviderGuid, ProviderDesignation, ReferencedPerson) " +
                $"VALUES (" +
                $"{order.DateTime.Ticks}, " +
                $"{order.Money.Euro}, " +
                $"{order.Money.Cent}, " +
                $"'{order.Provider.Guid}', " +
                $"'{order.Provider.Designation}', " +
                $"'{order.ReferencedPerson}');";
            QueryExecuterAtMySql(queryStringInsert);
            Log.Information(queryStringInsert + " - inserted");

            // update person saldo
            string queryStringUpdateSaldo = $"" +
                $"SELECT * " +
                $"FROM essenskasse.Person " +
                $"WHERE Guid='{order.ReferencedPerson}';";
            DataTable dataTablePersonToUpdate = QueryExecuterAtMySql(queryStringUpdateSaldo);

            if (dataTablePersonToUpdate.Rows.Count == 1) {
                Person personToUpdate = DataTableToPerson(dataTablePersonToUpdate);
                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} old Saldo = {personToUpdate.Saldo}");
                personToUpdate.Saldo = new Money().CalculateMoney(CalculationOperation.Substraction, personToUpdate.Saldo, order.Money)!;
                Log.Information($"Person {personToUpdate.Guid} {personToUpdate.Name} new Saldo = {personToUpdate.Saldo}");

                string queryStringPersonUpdate = $"" +
                    $"UPDATE essenskasse.Person " +
                    $"SET SaldoEuro = {personToUpdate.Saldo.Euro}, SaldoCent = {personToUpdate.Saldo.Cent} " +
                    $"WHERE Guid='{personToUpdate.Guid}';";
                QueryExecuterAtMySql(queryStringPersonUpdate);

                Log.Information($"New Order of {order.Money} for Person added.");
                return 0;
            }
            return 2;
        }
        #endregion
    }
}
