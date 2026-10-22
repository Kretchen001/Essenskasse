using EssenskasseAPI.Data;
using EssenskasseAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/transactionprocedure")]
    [RequireSession]
    public class TransactionProceduresController : ControllerBase {

        #region Config
        private readonly Db _db;

        public TransactionProceduresController(IConfiguration configuration) {
            _db = new Db(configuration);
        }
        #endregion

        #region Controller
        // Die Kaffeekasse darf ohne Anmeldung angesehen werden (reine Anzeige); Eintragen verlangt weiterhin eine Sitzung.
        [AllowAnonymous, HttpGet, Route("all")]
        public IActionResult TransactionProcedure() {
            using (Operation.Time("TransactionProcedure")) {
                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.TransactionProcedure " +
                    $"ORDER BY DateTime DESC;";

                DataTable dataTableTransaction = _db.Query(queryString);
                List<TransactionProcedure> transactions = DataTableToListOfTransactionProcedure(dataTableTransaction);

                return Ok(JsonConvert.SerializeObject(transactions));
            }
        }

        [HttpPost, Route("new")]
        public IActionResult NewOne([FromBody] TransactionProcedure newOne) {
            using (Operation.Time("NewOne at Procedures")) {
                if (newOne is null || newOne.Amount is null || newOne.Amount.ToTotalCents() == 0) {
                    return BadRequest();
                }
                string queryString = $"" +
                    $"INSERT INTO essenskasse.TransactionProcedure (DateTime, AmountEuro, AmountCent, Comment) " +
                    $"VALUES (@dateTime, @euro, @cent, @comment);";

                try {
                    _db.Query(queryString,
                        Db.P("@dateTime", newOne.DateTime.Ticks),
                        Db.P("@euro", newOne.Amount.Euro),
                        Db.P("@cent", newOne.Amount.Cent),
                        Db.P("@comment", newOne.Comment));
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
