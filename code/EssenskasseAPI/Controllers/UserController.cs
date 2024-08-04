using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
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
                    $"'{Uri.UnescapeDataString(user.Name)}', " +
                    $"'{Uri.UnescapeDataString(user.UserName)}', " +
                    $"'{Uri.UnescapeDataString(user.Pwd)}', " +
                    $"'{Uri.UnescapeDataString(user.Email)}', " +
                    $"{user.Role});";
                QueryExecuterAtMySql(queryStringInsert);

                return Ok($"Benutzer {user.Name} angelegt");
            }
        }

        [HttpPut, Route("update")]
        public IActionResult UpdateUser([FromHeader] Guid guid = new Guid(),
                                                [FromHeader] string name = "", [FromHeader] string userName = "",
                                                [FromHeader] string pwd = "", [FromHeader] string email = "",
                                                [FromHeader] int role = short.MaxValue) {
            using (Operation.Time("UpdateUser")) {
                Log.Information("UpdateUser called");

                string queryStringRequestUser = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.User " +
                    $"WHERE Guid='{guid}';";
                DataTable dataTableUser = QueryExecuterAtMySql(queryStringRequestUser);

                if (dataTableUser.Rows.Count == 1) {
                    U.User userToUpdate = DataTableToListOfUser(dataTableUser)[0];
                    if (name != "") {
                        Log.Information($"{userToUpdate.Name} to |{name}| change");
                        userToUpdate.Name = Uri.UnescapeDataString(name);
                    }
                    if (userName != "") {
                        Log.Information($"{userToUpdate.UserName} to |{userName}| change");
                        userToUpdate.UserName = Uri.UnescapeDataString(userName);
                    }
                    if (pwd != "") {
                        Log.Information($"{userToUpdate.Pwd} to |{pwd}| change");
                        userToUpdate.Pwd = Uri.UnescapeDataString(pwd);
                    }
                    if (email != "") {
                        Log.Information($"{userToUpdate.Email} to |{email}| change");
                        userToUpdate.Email = Uri.UnescapeDataString(email);
                    }
                    if (role < short.MaxValue) { // "echt kleiner gleich"
                        Log.Information($"{userToUpdate.Role} to |{role}| change");
                        userToUpdate.Role = role;
                    }

                    string queryStringUpdate = $"" +
                        $"UPDATE essenskasse.User " +
                        $"SET Name='{userToUpdate.Name}', UserName='{userToUpdate.UserName}', " +
                        $"Pwd='{userToUpdate.Pwd}', Email='{userToUpdate.Email}', Role='{userToUpdate.Role}'" +
                        $"WHERE Guid='{guid}';";
                    QueryExecuterAtMySql(queryStringUpdate);

                    Log.Information("User successfully updated");
                    return Ok();
                }
                else {
                    Log.Error($"UpdateUser failed, requested name = '{name}'");
                    return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                }
            }
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
            using (Operation.Time($"Check Token |{sessionToken}| for: {name}")) {
                string queryStringRequestName = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.User " +
                    $"WHERE Name='{name}';";
                DataTable dataTableUser = QueryExecuterAtMySql(queryStringRequestName);
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1) {
                    string queryStringRequestToken = $"" +
                        $"SELECT ExpaireDate " +
                        $"FROM essenskasse.SessionToken " +
                        $"WHERE Guid='{sessionToken}' AND " +
                        $"UserGuid='{user[0].Guid}';";
                    DataTable tempRes = QueryExecuterAtMySql(queryStringRequestToken);
                    if (tempRes.Rows.Count == 1) {
                        long s = long.Parse(tempRes.Rows[0]["ExpaireDate"].ToString()!);
                        if (DateTime.Now.Ticks < s) { // is under expaire
                            if (DateTime.Now.AddDays(4).Ticks > s) { // get new one
                                U.SessionToken newS = new U.SessionToken(Guid.NewGuid(), user[0].Guid, DateTime.Now.AddDays(14).Ticks);
                                string queryStringInsertToken = $"" +
                                    $"INSERT INTO essenskasse.SessionToken (Guid, UserGuid, ExpaireDate) " +
                                    $"VALUES (" +
                                    $"'{newS.Guid}'," +
                                    $"'{newS.UserGuid}'," +
                                    $"{newS.ExpaireDate});";
                                QueryExecuterAtMySql(queryStringInsertToken);
                                return Ok(newS.Guid); // return new Token because under 4 day barrier
                            }
                            return Ok(); // return without new Token because it is not time for it
                        }
                        else {
                            return Ok();
                        }
                    }
                }
                return NoContent();
            }
        }

        [HttpPost, Route("saveSettingsForUser")]
        public IActionResult SaveSettingsForUser([FromHeader] Guid sessionToken, [FromHeader] string name, [FromBody] U.Settings settings) {
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
