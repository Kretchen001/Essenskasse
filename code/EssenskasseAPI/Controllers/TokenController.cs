using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase {

        #region Config
        private readonly IConfiguration Configuration;

        public TokenController(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        #region Controller
        [HttpPut, Route("requestNewAccessToken")]
        public IActionResult RequestNewAccessToken([FromHeader] string? identity) {
            Log.Information("RequestNewAccessToken called");
            if (identity == null) {
                Log.Warning("No identity found!");
                return NoContent();
            }
            long a = DateTime.Now.Ticks;
            Guid guid = Guid.NewGuid();

            string queryStringInsert = $"" +
                $"INSERT INTO essenskasse.TokenForChange (DateTime, Guid, Identity) " +
                $"VALUES (" +
                $"{a}, '{guid}', '{identity}');";
            QueryExecuterAtMySql(queryStringInsert);

            return Ok(JsonConvert.SerializeObject(guid));
        }

        [HttpGet, Route("checkAccessToken")]
        public IActionResult CheckAccessToken([FromHeader] Guid? accessToken, [FromQuery] bool s) {
            using (Operation.Time("CheckAccessToken")) {
                if (s) {
                    Log.Information("CheckAccessToken called");
                    if (accessToken is null) {
                        Log.Information("CheckAccessToken called, but no token in Header");
                        return NoContent();
                    }
                    string queryStringInsert = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.TokenForChange " +
                        $"WHERE Guid='{accessToken}';";
                    DataTable datatableInsert = QueryExecuterAtMySql(queryStringInsert);
                    List<Tuple<DateTime, Guid>> res = DataTableToListOfPerson(datatableInsert);
                    if (res.Count == 1) {
                        Log.Information("CheckAccessToken called successfull");
                        return Ok();
                    }
                    Log.Warning("CheckAccessToken called NOT authorized!");
                    return BadRequest();
                }
                Log.Information("Query-Address miss bool");
                return BadRequest();
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

        private static List<Tuple<DateTime, Guid>> DataTableToListOfPerson(DataTable dataTable) {
            List<Tuple<DateTime, Guid>> tokenList = [];
            foreach (DataRow row in dataTable.Rows) {
                tokenList.Add(new Tuple<DateTime, Guid>(
                    item1: new DateTime(long.Parse(row["DateTime"].ToString()!)),
                    item2: new Guid(row["Guid"].ToString()!)
                    ));
            }
            return tokenList;
        }
        #endregion
    }
}
