using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using SerilogTimings;
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
            if (name.Equals("") || userName.Equals("") || pwd.Equals("") || email.Equals("") || role.Equals(short.MaxValue)) {
                return BadRequest("Notwendiger Parameter fehlt!");
            }
            using (Operation.Time($"CreateNewUser {name}, {userName}, {pwd}, {email}, {role}")) {
                U.User user = new U.User(name, userName, pwd, email, role);
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.User (Guid, Name, UserName, Pwd, Email, Role) " +
                    $"VALUES (" +
                    $"'{user.Guid}'," +
                    $"'{user.Name}', " +
                    $"'{user.UserName}', " +
                    $"'{user.Pwd}', " +
                    $"'{user.Email}', " +
                    $"{user.Role});";
                QueryExecuterAtMySql(queryStringInsert);

                return Ok($"Benutzer {user.Name} angelegt");
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdateUser([FromHeader] string name = "", [FromHeader] string userName = "",
                                                [FromHeader] string pwd = "", [FromHeader] string email = "",
                                                [FromHeader] int role = short.MaxValue) {
            return Conflict("not implemented yet!");
            //if (name.Equals("") || userName.Equals("") || pwd.Equals("") || email.Equals("") || role.Equals(short.MaxValue)) {
            //    return BadRequest("Notwendiger Parameter fehlt!");
            //}
            //using (Operation.Time($"CreateNewUser {name}, {userName}, {pwd}, {email}, {role}")) {
            //    U.User user = new U.User(name, userName, pwd, email, role);
            //    string queryStringInsert = $"" +
            //        $"UPDATE essenskasse.User " +
            //        $"SET Name='{name}', UserName='{userName}', Pwd='{pwd}', Email='{email}', Role='{role}'" +
            //        $"WHERE Guid='{guid}';";
            //    QueryExecuterAtMySql(queryStringInsert);

            //    return Ok($"Benutzer {user.Name} angelegt");
            //}
        }

        [HttpGet, Route("exist")]
        public IActionResult CheckIfExist([FromHeader] string name = "", [FromHeader] string userName = "",
                                            [FromHeader] string email = "", [FromHeader] bool withResult = false) {
            using (Operation.Time($"Check if User exist: {name}, {userName}, {email}")) {
                string queryStringInsert = $"" +
                    $"SELECT Name, UserName, Email, Role " +
                    $"FROM essenskasse.User " +
                    $"WHERE Name='{name}' OR " +
                    $"UserName='{userName}' OR" +
                    $"Email='{email}';";
                DataTable dataTableUser = QueryExecuterAtMySql(queryStringInsert);
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (withResult) {
                    return Ok(JsonConvert.SerializeObject(user));
                }
                else {
                    if (user.Count > 0) {
                        return Ok();
                    }
                    else {
                        return NoContent();
                    }
                }
            }
        }

        [HttpPost, Route("newTokenForUser")]
        public IActionResult GetNewSessionToken([FromHeader] string name = "", [FromHeader] string pwd = "",
                                                    [FromHeader] int role = short.MaxValue) {
            using (Operation.Time($"Get Token for: {name}")) {
                string queryStringInsert = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.User " +
                    $"WHERE Name='{name}' AND " +
                    $"Pwd='{pwd}';";
                DataTable dataTableUser = QueryExecuterAtMySql(queryStringInsert);
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1) {
                    U.SessionToken token = new U.SessionToken(Guid.NewGuid(), user[0].Guid, DateTime.Now.AddDays(14).Ticks);
                    string queryStringInsertToken = $"" +
                        $"INSERT INTO essenskasse.SessionToken (Guid, UserGuid, ExpaireDate) " +
                        $"VALUES (" +
                        $"'{token.Guid}'," +
                        $"'{token.UserGuid}'," +
                        $"{token.ExpaireDate});";
                    QueryExecuterAtMySql(queryStringInsertToken);
                    return Ok(token.Guid);
                }
                else {
                    return NoContent();
                }
            }
        }

        [HttpGet, Route("checkTokenForUser")]
        public IActionResult CheckSessionToken([FromHeader] string name = "", [FromHeader] Guid sessionToken = default) {
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
