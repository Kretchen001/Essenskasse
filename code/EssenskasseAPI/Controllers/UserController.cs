using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using U = Modells.User;

namespace EssenskasseAPI.Controllers {

    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase {

        #region Config
        private readonly IConfiguration Configuration;

        public UserController(IConfiguration configuration) {
            Configuration = configuration;
        }
        #endregion

        #region Controller
        [HttpPost, Route("new")]
        public IActionResult CreateNewUser([FromHeader] string name = "", [FromHeader] string userName = "",
                                                [FromHeader] string pwd = "", [FromHeader] string email = "",
                                                [FromHeader] int role = short.MaxValue) {
            return Ok();
        }

        [HttpGet, Route("exist")]
        public IActionResult CheckIfExist([FromHeader] string name = "", [FromHeader] string userName = "",
                                            [FromHeader] string email = "") {
            return Ok();
        }

        [HttpPost, Route("newTokenForUser")]
        public IActionResult GetNewSessionToken([FromHeader] string name = "", [FromHeader] string pwd = "", 
                                                    [FromHeader] int role = short.MaxValue) {
            return Ok();
        }

        [HttpGet, Route("checkTokenForUser")]
        public IActionResult CheckSessionToken([FromHeader] string name = "", [FromHeader] Guid sessionToken) {
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

        private static List<U.User> DataTableToListOfUser(DataTable dataTable) {
            List<U.User> userList = [];
            foreach (DataRow row in dataTable.Rows) {
                userList.Add(new U.User() {
                    Guid = new Guid(row["Guid"].ToString()!),
                    Name = row["Name"].ToString()!,
                    UserName = row["UserName"].ToString()!,
                    Pwd = row["Pwd"].ToString()!,
                    Email = row["Email"].ToString()!,
                    Role = int.Parse(row["Role"].ToString()!)
                });
            }
            return userList;
        }
        #endregion
    }
}
