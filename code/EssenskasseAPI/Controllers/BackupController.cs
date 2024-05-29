using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/backup")]
    public class BackupController : ControllerBase {

        #region Config
        private readonly IConfiguration Configuration;

        public BackupController(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        #region Controller
        [HttpPost, Route("newNow")]
        public IActionResult CreateNewBackupFromDb() {
            string identifier = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");

            //Json-Safe
            if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}backup")) {
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}backup");
            }
            string dirForBackup = $"{AppDomain.CurrentDomain.BaseDirectory}backup/{identifier}";
            Directory.CreateDirectory(dirForBackup);

            //request everything
            List<Person> persons = DataTableToListOfPerson(QueryExecuterAtMySql("SELECT * FROM essenskasse.Person"));
            List<Tuple<Provider, bool>> providers = DataTableToListOfProvider(QueryExecuterAtMySql("SELECT * FROM essenskasse.Provider"));
            List<Payment> payments = DataTableToListOfPayments(QueryExecuterAtMySql("SELECT * FROM essenskasse.Payment"));
            List<Order> orders = DataTableToListOfOrders(QueryExecuterAtMySql("SELECT * FROM essenskasse.Order"));
            List<TransactionProcedure> transactionProcedures = DataTableToListOfTransactionProcedure(QueryExecuterAtMySql("SELECT * FROM essenskasse.TransactionProcedure"));
            List<Tuple<DateTime, Guid, string>> tokens = DataTableToListOfToken(QueryExecuterAtMySql("SELECT * FROM essenskasse.TokenForChange"));

            //save all as json
            System.IO.File.WriteAllText(dirForBackup + "/person.json", JsonConvert.SerializeObject(persons));
            System.IO.File.WriteAllText(dirForBackup + "/provider.json", JsonConvert.SerializeObject(providers));
            System.IO.File.WriteAllText(dirForBackup + "/payment.json", JsonConvert.SerializeObject(payments));
            System.IO.File.WriteAllText(dirForBackup + "/order.json", JsonConvert.SerializeObject(orders));
            System.IO.File.WriteAllText(dirForBackup + "/transactionProcedure.json", JsonConvert.SerializeObject(transactionProcedures));
            System.IO.File.WriteAllText(dirForBackup + "/token.json", JsonConvert.SerializeObject(tokens));

            return Ok(identifier);
        }

        [HttpPost, Route("restore")]
        public IActionResult RestoreSpecificBackup([FromHeader] string identifier) {
            // identifier -> "yyyy_MM_dd_HH_mm"
            string dirForBackup = $"{AppDomain.CurrentDomain.BaseDirectory}backup/{identifier}";

            //read all json's
            List<Person> persons = JsonConvert.DeserializeObject<List<Person>>(System.IO.File.ReadAllText(dirForBackup + "/person.json")) ?? [];
            List<Tuple<Provider, bool>> providers = JsonConvert.DeserializeObject<List<Tuple<Provider, bool>>>(System.IO.File.ReadAllText(dirForBackup + "/provider.json")) ?? [];
            List<Payment> payments = JsonConvert.DeserializeObject<List<Payment>>(System.IO.File.ReadAllText(dirForBackup + "/payment.json")) ?? [];
            List<Order> orders = JsonConvert.DeserializeObject<List<Order>>(System.IO.File.ReadAllText(dirForBackup + "/order.json")) ?? [];
            List<TransactionProcedure> transactionProcedures = JsonConvert.DeserializeObject<List<TransactionProcedure>>(System.IO.File.ReadAllText(dirForBackup + "/transactionProcedure.json")) ?? [];
            List<Tuple<DateTime, Guid, string>> tokens = JsonConvert.DeserializeObject<List<Tuple<DateTime, Guid, string>>>(System.IO.File.ReadAllText(dirForBackup + "/token.json")) ?? [];

            //restore
            QueryExecuterAtMySql("TRUNCATE essenskasse.Person;");
            foreach (Person x in persons) {
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.Person (Guid, Name, Email, SaldoEuro, SaldoCent) " +
                    $"VALUES (" +
                    $"'{x.Guid}'," +
                    $"'{x.Name}', " +
                    $"'{x.Email}', " +
                    $"{x.Saldo.Euro}, " +
                    $"{x.Saldo.Cent});";
                QueryExecuterAtMySql(queryStringInsert);
            }
            QueryExecuterAtMySql("TRUNCATE essenskasse.Payment;");
            foreach (Payment x in payments) {
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.Payment (DateTime, AmountEuro, AmountCent, ReferencedPerson, Comment) " +
                    $"VALUES (" +
                    $"{x.DateTime.Ticks}, " +
                    $"{x.Amount.Euro}, " +
                    $"{x.Amount.Cent}, " +
                    $"'{x.ReferencedPerson}', " +
                    $"'{x.Comment}');";
                QueryExecuterAtMySql(queryStringInsert);
            }
            QueryExecuterAtMySql("TRUNCATE essenskasse.Provider;");
            foreach (Tuple<Provider, bool> x in providers) {
                string v = "false";
                if (x.Item2) {
                    v = "true";
                }
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.Provider (Guid, Designation, Sichtbar)" +
                    $"VALUES (" +
                    $"'{x.Item1.Guid}', " +
                    $"'{x.Item1.Designation}', " +
                    $"{v})";
                QueryExecuterAtMySql(queryStringInsert);
            }
            QueryExecuterAtMySql("TRUNCATE essenskasse.Order;");
            foreach (Order x in orders) {
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.Order (DateTime, AmountEuro, AmountCent, ProviderGuid, ProviderDesignation, ReferencedPerson) " +
                    $"VALUES (" +
                    $"{x.DateTime.Ticks}, " +
                    $"{x.Money.Euro}, " +
                    $"{x.Money.Cent}, " +
                    $"'{x.Provider.Guid}', " +
                    $"'{x.Provider.Designation}', " +
                    $"'{x.ReferencedPerson}');";
                QueryExecuterAtMySql(queryStringInsert);
            }
            QueryExecuterAtMySql("TRUNCATE essenskasse.TransactionProcedure");
            foreach (TransactionProcedure x in transactionProcedures) {
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.TransactionProcedure (DateTime, AmountEuro, AmountCent, Comment) " +
                    $"VALUES (" +
                    $"{x.DateTime.Ticks}, " +
                    $"{x.Amount.Euro}, " +
                    $"{x.Amount.Cent}, " +
                    $"'{x.Comment}');";
                QueryExecuterAtMySql(queryStringInsert);
            }
            QueryExecuterAtMySql("TRUNCATE essenskasse.TokenForChange;");
            foreach (Tuple<DateTime, Guid, string> x in tokens) {
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.TokenForChange (DateTime, Guid, Identity) " +
                    $"VALUES (" +
                    $"{x.Item1.Ticks}, " +
                    $"'{x.Item2}', " +
                    $"'{x.Item3}');";
                QueryExecuterAtMySql(queryStringInsert);
            }

            return Ok();
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

        private static List<Person> DataTableToListOfPerson(DataTable dataTable) {
            List<Person> personList = [];
            foreach (DataRow row in dataTable.Rows) {
                personList.Add(new Person(
                    guid: new Guid(row["Guid"].ToString()!),
                    name: Uri.EscapeDataString(row["Name"].ToString()!),
                    email: Uri.EscapeDataString(row["Email"].ToString()!),
                    money: new Money(
                        euro: long.Parse(row["SaldoEuro"].ToString()!),
                        cent: short.Parse(row["SaldoCent"].ToString()!)
                        )
                    ));
            }
            return personList;
        }

        private static List<Tuple<Provider, bool>> DataTableToListOfProvider(DataTable dataTable) {
            List<Tuple<Provider, bool>> providerController = [];
            foreach (DataRow row in dataTable.Rows) {
                Provider temp = new Provider() {
                    Guid = new Guid(row["Guid"].ToString()!),
                    Designation = row["Designation"].ToString()!
                };
                if (row["Sichtbar"].ToString()!.Equals("True")) {
                    providerController.Add(new Tuple<Provider, bool>(temp, true));
                }
                else {
                    providerController.Add(new Tuple<Provider, bool>(temp, false));
                }
            }
            return providerController;
        }

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

        private static List<Tuple<DateTime, Guid, string>> DataTableToListOfToken(DataTable dataTable) {
            List<Tuple<DateTime, Guid, string>> tokenList = [];
            foreach (DataRow row in dataTable.Rows) {
                tokenList.Add(new Tuple<DateTime, Guid, string>(
                    item1: new DateTime(long.Parse(row["DateTime"].ToString()!)),
                    item2: new Guid(row["Guid"].ToString()!),
                    item3: row["Identity"].ToString()!
                    ));
            }
            return tokenList;
        }

        private static List<TransactionProcedure> DataTableToListOfTransactionProcedure(DataTable dataTable) {
            List<TransactionProcedure> providerController = [];
            foreach (DataRow row in dataTable.Rows) {
                providerController.Add(new TransactionProcedure() {
                    DateTime = new DateTime(long.Parse(row["DateTime"].ToString()!)),
                    Amount = new Money() {
                        Euro = long.Parse(row["AmountEuro"].ToString()!),
                        Cent = short.Parse(row["AmountCent"].ToString()!)
                    },
                    Comment = row["Comment"].ToString()!
                });
            }
            return providerController;
        }
        #endregion
    }
}
