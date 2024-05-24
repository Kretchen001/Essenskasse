using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics.CodeAnalysis;

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
        public IActionResult CreateNewBackupFromDb([FromQuery] int format) {
            if (format == 0) {
                //Json-Safe
                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}backup")) {
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}backup");
                }
                string dirForBackup = $"{AppDomain.CurrentDomain.BaseDirectory}backup/{DateTime.Now.ToString("yyyy_MM_dd_HH_mm")}";
                Directory.CreateDirectory(dirForBackup);
                //request everything
                List<Person> persons = DataTableToListOfPerson(QueryExecuterAtMySql("SELECT * FROM essenskasse.Person"));
                List<Provider> providers = DataTableToListOfProvider(QueryExecuterAtMySql("SELECT * FROM essenskasse.Provider"));
                List<Payment> payments = DataTableToListOfPayments(QueryExecuterAtMySql("SELECT * FROM essenskasse.Payment"));
                List<Order> orders = DataTableToListOfOrders(QueryExecuterAtMySql("SELECT * FROM essenskasse.Order"));
                List<TransactionProcedure> transactionProcedures = DataTableToListOfTransactionProcedure(QueryExecuterAtMySql("SELECT * FROM essenskasse.TransactionProcedure"));
                List<Tuple<DateTime, Guid>> tokens = DataTableToListOfToken(QueryExecuterAtMySql("SELECT * FROM essenskasse.TokenForChange"));
                //save all as json
                System.IO.File.WriteAllText(dirForBackup + "/person.json", JsonConvert.SerializeObject(persons));
                System.IO.File.WriteAllText(dirForBackup + "/provider.json", JsonConvert.SerializeObject(providers));
                System.IO.File.WriteAllText(dirForBackup + "/payment.json", JsonConvert.SerializeObject(payments));
                System.IO.File.WriteAllText(dirForBackup + "/order.json", JsonConvert.SerializeObject(orders));
                System.IO.File.WriteAllText(dirForBackup + "/transactionProcedure.json", JsonConvert.SerializeObject(transactionProcedures));
                System.IO.File.WriteAllText(dirForBackup + "/token.json", JsonConvert.SerializeObject(tokens));
            }
            else {
                // current nothing
            }
            return Ok();
        }

        [HttpPost, Route("restore")]
        public IActionResult RestoreSpecificBackup(string identifier) {
            return Ok("Not implemented yet!");
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

        private static List<Provider> DataTableToListOfProvider(DataTable dataTable) {
            List<Provider> providerController = [];
            foreach (DataRow row in dataTable.Rows) {
                providerController.Add(new Provider() {
                    Guid = new Guid(row["Guid"].ToString()!),
                    Designation = row["Designation"].ToString()!
                });
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

        private static List<Tuple<DateTime, Guid>> DataTableToListOfToken(DataTable dataTable) {
            List<Tuple<DateTime, Guid>> tokenList = [];
            foreach (DataRow row in dataTable.Rows) {
                tokenList.Add(new Tuple<DateTime, Guid>(
                    item1: new DateTime(long.Parse(row["DateTime"].ToString()!)),
                    item2: new Guid(row["Guid"].ToString()!)
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
