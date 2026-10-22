using EssenskasseAPI.Data;
using EssenskasseAPI.Security;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
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
        private readonly Db _db;

        public UserController(IConfiguration configuration) {
            _db = new Db(configuration);
        }
        #endregion

        #region Controller
        // Ohne Sitzung darf nur der allererste Benutzer angelegt werden (Ersteinrichtung).
        // Danach duerfen nur Administratoren neue Benutzer anlegen.
        [Microsoft.AspNetCore.Authorization.AllowAnonymous, HttpPost, Route("new")]
        public IActionResult CreateNewUser([FromHeader] string name = "", [FromHeader] string userName = "",
                                                [FromHeader] string pwd = "", [FromHeader] string email = "",
                                                [FromHeader] int role = short.MaxValue) {
            if (name.Equals("") || userName.Equals("") || pwd.Equals("") || email.Equals("") || role.Equals(short.MaxValue)) {
                return BadRequest("Notwendiger Parameter fehlt!");
            }
            long existingUsers = Convert.ToInt64(_db.Query("SELECT COUNT(*) FROM essenskasse.User;").Rows[0][0]);
            if (existingUsers > 0) {
                SessionInfo? caller = SessionAuth.Authenticate(HttpContext);
                if (caller is null) {
                    return Unauthorized();
                }
                if (!SessionAuth.IsAdmin(HttpContext, caller)) {
                    return StatusCode(StatusCodes.Status403Forbidden);
                }
            }
            using (Operation.Time($"CreateNewUser {name}, {userName}, {pwd}, {email}, {role}")) {
                U.User user = new U.User(name, userName, pwd, email, role);
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.User (Guid, Name, UserName, Pwd, Email, Role) " +
                    $"VALUES (@guid, @name, @userName, @pwd, @email, @role);";
                _db.Query(queryStringInsert,
                    Db.P("@guid", user.Guid),
                    Db.P("@name", Uri.UnescapeDataString(user.Name)),
                    Db.P("@userName", Uri.UnescapeDataString(user.UserName)),
                    Db.P("@pwd", Uri.UnescapeDataString(user.Pwd)),
                    Db.P("@email", Uri.UnescapeDataString(user.Email)),
                    Db.P("@role", user.Role));

                return Ok($"Benutzer {user.Name} angelegt");
            }
        }

        [RequireSession, HttpPut, Route("update")]
        public IActionResult UpdateUser([FromHeader] Guid guid = new Guid(),
                                                [FromHeader] string name = "", [FromHeader] string userName = "",
                                                [FromHeader] string pwd = "", [FromHeader] string email = "",
                                                [FromHeader] int role = short.MaxValue) {
            using (Operation.Time("UpdateUser")) {
                Log.Information("UpdateUser called");

                // Benutzer duerfen nur sich selbst aendern, Administratoren jeden.
                // Die Rolle darf nur ein Administrator aendern (sonst koennte sich jeder zum Admin machen).
                SessionInfo caller = SessionAuth.Authenticate(HttpContext)!;
                bool callerIsAdmin = SessionAuth.IsAdmin(HttpContext, caller);
                if (!callerIsAdmin && (caller.UserGuid != guid || role < short.MaxValue)) {
                    Log.Warning($"UpdateUser by {caller.UserGuid} for {guid} denied");
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                string queryStringRequestUser = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.User " +
                    $"WHERE Guid=@guid;";
                DataTable dataTableUser = _db.Query(queryStringRequestUser, Db.P("@guid", guid));

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
                        $"SET Name=@name, UserName=@userName, " +
                        $"Pwd=@pwd, Email=@email, Role=@role " +
                        $"WHERE Guid=@guid;";
                    _db.Query(queryStringUpdate,
                        Db.P("@name", userToUpdate.Name),
                        Db.P("@userName", userToUpdate.UserName),
                        Db.P("@pwd", userToUpdate.Pwd),
                        Db.P("@email", userToUpdate.Email),
                        Db.P("@role", userToUpdate.Role),
                        Db.P("@guid", guid));

                    Log.Information("User successfully updated");
                    return Ok();
                }
                else {
                    Log.Error($"UpdateUser failed, requested name = '{name}'");
                    return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                }
            }
        }

        [RequireSession, HttpGet, Route("exist")]
        public IActionResult CheckIfExist([FromHeader] string name = "", [FromHeader] string userName = "",
                                            [FromHeader] string email = "", [FromHeader] bool withResult = false) {
            using (Operation.Time($"Check if User exist: {name}, {userName}, {email}")) {
                string queryStringInsert = $"" +
                    $"SELECT Guid, Name, UserName, Pwd, Email, Role " +
                    $"FROM essenskasse.User " +
                    $"WHERE Name=@name OR " +
                    $"UserName=@userName OR " +
                    $"Email=@email;";
                DataTable dataTableUser = _db.Query(queryStringInsert,
                    Db.P("@name", name),
                    Db.P("@userName", userName),
                    Db.P("@email", email));
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (withResult) {
                    user.ForEach(x => x.Pwd = string.Empty); // das Passwort wird nie ausgeliefert
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
                    $"WHERE Name=@name AND " +
                    $"Pwd=@pwd;";
                DataTable dataTableUser = _db.Query(queryStringInsert,
                    Db.P("@name", name),
                    Db.P("@pwd", pwd));
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1) {
                    U.SessionToken token = new U.SessionToken(Guid.NewGuid(), user[0].Guid, DateTime.Now.AddDays(14).Ticks);
                    string queryStringInsertToken = $"" +
                        $"INSERT INTO essenskasse.SessionToken (Guid, UserGuid, ExpaireDate) " +
                        $"VALUES (@guid, @userGuid, @expaireDate);";
                    _db.Query(queryStringInsertToken,
                        Db.P("@guid", token.Guid),
                        Db.P("@userGuid", token.UserGuid),
                        Db.P("@expaireDate", token.ExpaireDate));
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
                    $"WHERE Name=@name;";
                DataTable dataTableUser = _db.Query(queryStringRequestName, Db.P("@name", name));
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1) {
                    string queryStringRequestToken = $"" +
                        $"SELECT ExpaireDate " +
                        $"FROM essenskasse.SessionToken " +
                        $"WHERE Guid=@sessionToken AND " +
                        $"UserGuid=@userGuid;";
                    DataTable tempRes = _db.Query(queryStringRequestToken,
                        Db.P("@sessionToken", sessionToken),
                        Db.P("@userGuid", user[0].Guid));
                    if (tempRes.Rows.Count == 1) {
                        long s = long.Parse(tempRes.Rows[0]["ExpaireDate"].ToString()!);
                        if (DateTime.Now.Ticks < s) { // is under expaire
                            if (DateTime.Now.AddDays(4).Ticks > s) { // get new one
                                U.SessionToken newS = new U.SessionToken(Guid.NewGuid(), user[0].Guid, DateTime.Now.AddDays(14).Ticks);
                                string queryStringInsertToken = $"" +
                                    $"INSERT INTO essenskasse.SessionToken (Guid, UserGuid, ExpaireDate) " +
                                    $"VALUES (@guid, @userGuid, @expaireDate);";
                                _db.Query(queryStringInsertToken,
                                    Db.P("@guid", newS.Guid),
                                    Db.P("@userGuid", newS.UserGuid),
                                    Db.P("@expaireDate", newS.ExpaireDate));
                                return Ok(newS.Guid); // return new Token because under 4 day barrier
                            }
                            return Ok(); // return without new Token because it is not time for it
                        }
                        else {
                            return BadRequest();
                        }
                    }
                }
                return NoContent();
            }
        }

        [HttpGet, Route("loadSettingsForUser")]
        public IActionResult LoadSettingsForUser([FromHeader] Guid sessionToken = default, [FromHeader] string name = "") {
            using (Operation.Time($"Load Settings for: {name}")) {
                string queryStringRequestName = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.User " +
                        $"WHERE Name=@name;";
                DataTable dataTableUser = _db.Query(queryStringRequestName, Db.P("@name", name));
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1) {
                    string queryStringRequestToken = $"" +
                        $"SELECT ExpaireDate " +
                        $"FROM essenskasse.SessionToken " +
                        $"WHERE Guid=@sessionToken AND " +
                        $"UserGuid=@userGuid;";
                    DataTable tempRes = _db.Query(queryStringRequestToken,
                        Db.P("@sessionToken", sessionToken),
                        Db.P("@userGuid", user[0].Guid));
                    if (tempRes.Rows.Count == 1) {
                        long s = long.Parse(tempRes.Rows[0]["ExpaireDate"].ToString()!);
                        if (DateTime.Now.Ticks < s) { // is under expaire
                            string queryStringRequestSettings = $"" +
                                //$"SELECT * " +
                                $"SELECT isSingleListWishDashboard, loadAmountPayment, loadAmountOrder, filterShowProvider, dashboardMoneyLimit, filterShowPerson, accessToken, adminEmail, noticeableOnRightSide, sendConfirmationMail " +
                                $"FROM essenskasse.SettingsUser " +
                                $"WHERE UserGuid=@userGuid;";
                            DataTable tempSettings = _db.Query(queryStringRequestSettings, Db.P("@userGuid", user[0].Guid));
                            return Ok(DataTableToSettings(tempSettings));
                        }
                        Log.Information("User expire token");
                    }
                    Log.Warning("User found, but tempRes empty");
                }
                Log.Warning("Not loaded!");
                return BadRequest();
            }
        }

        [HttpPost, Route("saveSettingsForUser")]
        public IActionResult SaveSettingsForUser([FromHeader] Guid sessionToken = default, [FromHeader] string name = "", [FromBody] U.Settings? settings = null) {
            using (Operation.Time($"Save Settings for: {name}")) {
                if (settings is null) {
                    Log.Information("Request without Settings-Content.");
                    return BadRequest();
                }
                string queryStringRequestName = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.User " +
                        $"WHERE Name=@name;";
                DataTable dataTableUser = _db.Query(queryStringRequestName, Db.P("@name", name));
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1) { // user exist
                    string queryStringRequestToken = $"" +
                        $"SELECT ExpaireDate " +
                        $"FROM essenskasse.SessionToken " +
                        $"WHERE Guid=@sessionToken AND " +
                        $"UserGuid=@userGuid;";
                    DataTable tempRes = _db.Query(queryStringRequestToken,
                        Db.P("@sessionToken", sessionToken),
                        Db.P("@userGuid", user[0].Guid));
                    if (tempRes.Rows.Count == 1) {
                        long s = long.Parse(tempRes.Rows[0]["ExpaireDate"].ToString()!);
                        if (DateTime.Now.Ticks < s) { // is under expaire
                            string queryCheckIfSettingsSaved = $"" +
                                $"SELECT * " +
                                $"FROM essenskasse.SettingsUser " +
                                $"WHERE UserGuid=@userGuid;";
                            DataTable tempSettings = _db.Query(queryCheckIfSettingsSaved, Db.P("@userGuid", user[0].Guid));
                            settings.AccessToken ??= new Guid("00000000-0000-0000-0000-000000000000");
                            MySqlParameter[] settingsParameters = [
                                Db.P("@userGuid", user[0].Guid),
                                Db.P("@singleList", settings.IsSingleListWishDashboard),
                                Db.P("@loadPayment", settings.LoadAmountPayment),
                                Db.P("@loadOrder", settings.LoadAmountOrder),
                                Db.P("@showProvider", settings.FilterShowProvider),
                                Db.P("@moneyLimit", settings.DashboardMoneyLimit.ToString()),
                                Db.P("@showPerson", settings.FilterShowPerson),
                                Db.P("@accessToken", settings.AccessToken),
                                Db.P("@adminEmail", settings.AdminEmail),
                                Db.P("@rightSide", settings.NoticeableOnRightSide),
                                Db.P("@sendMail", settings.SendConfirmationMail)
                            ];
                            if (tempSettings.Rows.Count == 0) { // no settings there, so insert
                                string insertQueryString = $"" +
                                    $"INSERT INTO essenskasse.SettingsUser (UserGuid, isSingleListWishDashboard, " +
                                                $"loadAmountPayment, loadAmountOrder, filterShowProvider, " +
                                                $"dashboardMoneyLimit, filterShowPerson, accessToken, " +
                                                $"adminEmail, noticeableOnRightSide, sendConfirmationMail) " +
                                    $"VALUES (@userGuid, @singleList, @loadPayment, @loadOrder, @showProvider, " +
                                                $"@moneyLimit, @showPerson, @accessToken, " +
                                                $"@adminEmail, @rightSide, @sendMail);";
                                _db.Query(insertQueryString, settingsParameters);
                            }
                            else { // settings saved, so update
                                string queryUpdate = $"" +
                                $"UPDATE essenskasse.SettingsUser " +
                                    $"SET " +
                                    $"isSingleListWishDashboard=@singleList, " +
                                    $"loadAmountPayment=@loadPayment, " +
                                    $"loadAmountOrder=@loadOrder, " +
                                    $"filterShowProvider=@showProvider, " +
                                    $"dashboardMoneyLimit=@moneyLimit, " +
                                    $"filterShowPerson=@showPerson, " +
                                    $"accessToken=@accessToken, " +
                                    $"adminEmail=@adminEmail, " +
                                    $"noticeableOnRightSide=@rightSide, " +
                                    $"sendConfirmationMail=@sendMail " +
                                    $"WHERE UserGuid=@userGuid;";
                                _db.Query(queryUpdate, settingsParameters);
                            }
                            return Ok();
                        }
                        else {
                            Log.Warning("Not saved! (expiredate!)");
                            return BadRequest();
                        }
                    }
                }
                Log.Warning("Not saved! (no user found)");
                return BadRequest();
            }
        }

        [HttpGet, Route("accessToken/get")]
        public IActionResult RequestAccessTokenForUser([FromHeader] Guid sessionToken = default, [FromHeader] string name = "") {
            using (Operation.Time($"RequestAccessTokenForUser called")) {
                string queryStringRequestName = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.User " +
                        $"WHERE Name=@name;";
                DataTable dataTableUser = _db.Query(queryStringRequestName, Db.P("@name", name));
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1) {
                    string queryStringRequestToken = $"" +
                        $"SELECT ExpaireDate " +
                        $"FROM essenskasse.SessionToken " +
                        $"WHERE Guid=@sessionToken AND " +
                        $"UserGuid=@userGuid;";
                    DataTable tempRes = _db.Query(queryStringRequestToken,
                        Db.P("@sessionToken", sessionToken),
                        Db.P("@userGuid", user[0].Guid));
                    if (tempRes.Rows.Count == 1) {
                        long s = long.Parse(tempRes.Rows[0]["ExpaireDate"].ToString()!);
                        if (DateTime.Now.Ticks < s) { // is under expaire
                            //check if User has no token, so create new and send them
                            if (user[0].TokenForChange.Equals(new Guid("00000000-0000-0000-0000-000000000000"))) {
                                Guid newToken = Guid.NewGuid();
                                string queryInsertNewToken = $"" +
                                    $"UPDATE essenskasse.User " +
                                    $"SET TokenForChange=@newToken " +
                                    $"WHERE Guid=@userGuid;";
                                _db.Query(queryInsertNewToken,
                                    Db.P("@newToken", newToken),
                                    Db.P("@userGuid", user[0].Guid));
                                return Ok(newToken);
                            }
                            else { // send the "old" token
                                return Ok(user[0].TokenForChange);
                            }
                        }
                    }
                    Log.Warning($"{name} called with wrong sessionToken!");
                }
                else {
                    Log.Information($"{name} has called, but not exist.");
                }
            }
            return BadRequest();
        }
        #endregion

        #region InternalFunctions
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

        private static U.Settings DataTableToSettings(DataTable dataTable) {
            Guid? accessTokenGuid = null;
            if (dataTable.Rows[0]["accessToken"].ToString()! != "00000000-0000-0000-0000-000000000000") {
                accessTokenGuid = new Guid(dataTable.Rows[0]["accessToken"].ToString()!);
            }
            return new U.Settings(
                bool.Parse(dataTable.Rows[0]["isSingleListWishDashboard"].ToString() ?? "false"),
                int.Parse(dataTable.Rows[0]["loadAmountPayment"].ToString() ?? "25"),
                int.Parse(dataTable.Rows[0]["loadAmountOrder"].ToString() ?? "50"),
                bool.Parse(dataTable.Rows[0]["filterShowProvider"].ToString() ?? "false"),
                new Modells.Money().ToMoney(dataTable.Rows[0]["dashboardMoneyLimit"].ToString() ?? "0,00€"),
                bool.Parse(dataTable.Rows[0]["filterShowPerson"].ToString() ?? "false"),
                accessTokenGuid,
                dataTable.Rows[0]["adminEmail"].ToString() ?? "",
                bool.Parse(dataTable.Rows[0]["noticeableOnRightSide"].ToString() ?? "false"),
                bool.Parse(dataTable.Rows[0]["sendConfirmationMail"].ToString() ?? "true")
                );
        }
        #endregion
    }
}
