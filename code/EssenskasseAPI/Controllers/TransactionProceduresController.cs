using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/transactionprocedure")]
    public class TransactionProceduresController : ControllerBase {

        #region Config
        private readonly IConfiguration Configuration;

        public TransactionProceduresController(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        #region Controller
        [HttpGet, Route("all")]
        public IActionResult GetAllProcedures() {
            using (Operation.Time("GetAllProcedures")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.TransactionProcedure;";

                DataTable dataTableTransaction = QueryExecuterAtMySql(queryString);
                List<TransactionProcedure> transactions = DataTableToListOfProvider(dataTableTransaction);

                return Ok(JsonConvert.SerializeObject(transactions));
            }
        }

        [HttpPost, Route("new")]
        public IActionResult NewOne([FromBody] TransactionProcedure newOne) {
            using (Operation.Time("NewOne at Procedures")) {
                string queryString = $"" +
                    $"INSERT INTO essenskasse.TransactionProcedure (DateTime, AmountEuro, AmountCent, Comment) " +
                    $"VALUES (" +
                    $"{newOne.DateTime.Ticks}, " +
                    $"{newOne.Amount.Euro}, " +
                    $"{newOne.Amount.Cent}, " +
                    $"'{newOne.Comment}');";

                try {
                    QueryExecuterAtMySql(queryString);
                    Log.Information($"New Transaction inserted - {newOne.DateTime.Ticks} - {newOne.Amount.Euro} - {newOne.Amount.Cent} - {newOne.Comment}");
                    return Ok();
                }
                catch (Exception ex) {
                    Log.Error($"Insertion of Transaction failed - {ex.Message} - {ex.StackTrace}");
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

        private List<TransactionProcedure> DataTableToListOfProvider(DataTable dataTable) {
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
