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
        private static readonly string DummyHash = PasswordHasher.Hash("dummy");
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
            using (Operation.Time($"CreateNewUser {name}, {userName}, {email}, {role}")) {
                U.User user = new U.User(name, userName, pwd, email, role);
                string queryStringInsert = $"" +
                    $"INSERT INTO essenskasse.User (Guid, Name, UserName, Pwd, Email, Role) " +
                    $"VALUES (@guid, @name, @userName, @pwd, @email, @role);";
                _db.Query(queryStringInsert,
                    Db.P("@guid", user.Guid),
                    Db.P("@name", Uri.UnescapeDataString(user.Name)),
                    Db.P("@userName", Uri.UnescapeDataString(user.UserName)),
                    Db.P("@pwd", PasswordHasher.Hash(Uri.UnescapeDataString(user.Pwd))),
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
                        Log.Information($"Password of {userToUpdate.Name} changed");
                        userToUpdate.Pwd = PasswordHasher.Hash(Uri.UnescapeDataString(pwd));
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
                    $"WHERE Name=@name;";
                DataTable dataTableUser = _db.Query(queryStringInsert, Db.P("@name", name));
                List<U.User> user = DataTableToListOfUser(dataTableUser);
                if (user.Count == 1 && PasswordHasher.Verify(pwd, user[0].Pwd)) {
                    if (PasswordHasher.NeedsRehash(user[0].Pwd)) {
                        // Altbestand mit Klartext-Passwort: beim ersten erfolgreichen Login durch einen Hash ersetzen.
                        _db.Query("UPDATE essenskasse.User SET Pwd=@pwd WHERE Guid=@guid;",
                            Db.P("@pwd", PasswordHasher.Hash(pwd)),
                            Db.P("@guid", user[0].Guid));
                        Log.Information($"Password of {user[0].Name} migrated to hash");
                    }
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
                    if (user.Count == 0) {
                        // gleiche Rechenzeit wie bei einem vorhandenen Benutzer (keine Benutzernamen-Erkennung per Antwortzeit)
                        PasswordHasher.Verify(pwd, DummyHash);
                    }
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
                Guid? userGuid = ResolveUserOfSession(name, sessionToken);
                if (userGuid is null) {
                    Log.Warning("Settings not loaded (unknown user or invalid session)");
                    return BadRequest();
                }
                DataTable tempSettings = _db.Query(
                    "SELECT isSingleListWishDashboard, loadAmountPayment, loadAmountOrder, filterShowProvider, dashboardMoneyLimit, " +
                    "filterShowPerson, adminEmail, noticeableOnRightSide, sendConfirmationMail, " +
                    "quickAmounts, theme, language, personSort, providerSort, currency " +
                    "FROM essenskasse.SettingsUser WHERE UserGuid=@userGuid;",
                    Db.P("@userGuid", userGuid));
                if (tempSettings.Rows.Count == 0) {
                    // Noch nie gespeichert: kein Fehler, der Client arbeitet dann mit seinen Standardwerten weiter.
                    return NoContent();
                }
                return Ok(DataRowToSettings(tempSettings.Rows[0]));
            }
        }

        [HttpPost, Route("saveSettingsForUser")]
        public IActionResult SaveSettingsForUser([FromHeader] Guid sessionToken = default, [FromHeader] string name = "", [FromBody] U.Settings? settings = null) {
            using (Operation.Time($"Save Settings for: {name}")) {
                if (settings is null) {
                    Log.Information("Request without Settings-Content.");
                    return BadRequest();
                }
                Guid? userGuid = ResolveUserOfSession(name, sessionToken);
                if (userGuid is null) {
                    Log.Warning("Settings not saved (unknown user or invalid session)");
                    return BadRequest();
                }
                // Werte begrenzen und pruefen; der Aenderungstoken gehoert zum Benutzer und wird hier nie gespeichert.
                U.Settings clean = U.SettingsSanitizer.Sanitize(settings);
                // Einfuegen oder aktualisieren in einem Schritt: zwei gleichzeitige erste Speichervorgaenge scheitern nicht am Schluessel.
                _db.Query(
                    "INSERT INTO essenskasse.SettingsUser (UserGuid, isSingleListWishDashboard, loadAmountPayment, loadAmountOrder, " +
                    "filterShowProvider, dashboardMoneyLimit, filterShowPerson, accessToken, adminEmail, noticeableOnRightSide, " +
                    "sendConfirmationMail, quickAmounts, theme, language, personSort, providerSort, currency) " +
                    "VALUES (@userGuid, @singleList, @loadPayment, @loadOrder, @showProvider, @moneyLimit, @showPerson, @accessToken, " +
                    "@adminEmail, @rightSide, @sendMail, @quickAmounts, @theme, @language, @personSort, @providerSort, @currency) " +
                    "ON DUPLICATE KEY UPDATE " +
                    "isSingleListWishDashboard=VALUES(isSingleListWishDashboard), loadAmountPayment=VALUES(loadAmountPayment), " +
                    "loadAmountOrder=VALUES(loadAmountOrder), filterShowProvider=VALUES(filterShowProvider), " +
                    "dashboardMoneyLimit=VALUES(dashboardMoneyLimit), filterShowPerson=VALUES(filterShowPerson), " +
                    "accessToken=VALUES(accessToken), adminEmail=VALUES(adminEmail), noticeableOnRightSide=VALUES(noticeableOnRightSide), " +
                    "sendConfirmationMail=VALUES(sendConfirmationMail), quickAmounts=VALUES(quickAmounts), theme=VALUES(theme), " +
                    "language=VALUES(language), personSort=VALUES(personSort), providerSort=VALUES(providerSort), currency=VALUES(currency);",
                    Db.P("@userGuid", userGuid),
                    Db.P("@singleList", clean.IsSingleListWishDashboard),
                    Db.P("@loadPayment", clean.LoadAmountPayment),
                    Db.P("@loadOrder", clean.LoadAmountOrder),
                    Db.P("@showProvider", clean.FilterShowProvider),
                    Db.P("@moneyLimit", clean.DashboardMoneyLimit.ToString()),
                    Db.P("@showPerson", clean.FilterShowPerson),
                    Db.P("@accessToken", Guid.Empty),
                    Db.P("@adminEmail", clean.AdminEmail),
                    Db.P("@rightSide", clean.NoticeableOnRightSide),
                    Db.P("@sendMail", (int)clean.SendConfirmationMail),
                    Db.P("@quickAmounts", U.SettingsSanitizer.QuickAmountsToText(clean.QuickAmounts)),
                    Db.P("@theme", clean.Theme),
                    Db.P("@language", clean.Language),
                    Db.P("@personSort", clean.PersonSort),
                    Db.P("@providerSort", clean.ProviderSort),
                    Db.P("@currency", clean.Currency));
                return Ok();
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

        /// <summary>Prueft Name und Sitzungstoken. Gibt die Guid des Benutzers zurueck oder null, wenn der Benutzer
        /// unbekannt (oder der Name mehrdeutig) ist oder die Sitzung nicht zu ihm gehoert bzw. abgelaufen ist.</summary>
        private Guid? ResolveUserOfSession(string name, Guid sessionToken) {
            DataTable dataTableUser = _db.Query("SELECT Guid FROM essenskasse.User WHERE Name=@name;", Db.P("@name", name));
            if (dataTableUser.Rows.Count != 1) {
                return null;
            }
            Guid userGuid = new Guid(dataTableUser.Rows[0]["Guid"].ToString()!);
            DataTable tempRes = _db.Query(
                "SELECT ExpaireDate FROM essenskasse.SessionToken WHERE Guid=@sessionToken AND UserGuid=@userGuid;",
                Db.P("@sessionToken", sessionToken),
                Db.P("@userGuid", userGuid));
            if (tempRes.Rows.Count != 1) {
                return null;
            }
            long expires = long.Parse(tempRes.Rows[0]["ExpaireDate"].ToString()!);
            return DateTime.Now.Ticks < expires ? userGuid : null;
        }

        /// <summary>Liest eine Zeile der Einstellungstabelle. Leere Felder (NULL) ergeben die Standardwerte;
        /// das Ergebnis wird bereinigt, der Aenderungstoken ist immer leer.</summary>
        private static U.Settings DataRowToSettings(DataRow row) {
            Modells.Money limit;
            try {
                limit = new Modells.Money().ToMoney(ReadText(row["dashboardMoneyLimit"]));
            }
            catch {
                limit = new Modells.Money(0, 0);
            }
            U.ConfirmationMailMode mailMode = U.ConfirmationMailModes.Parse(ReadText(row["sendConfirmationMail"]));
            return U.SettingsSanitizer.Sanitize(new U.Settings {
                IsSingleListWishDashboard = ReadBool(row["isSingleListWishDashboard"], false),
                LoadAmountPayment = ReadInt(row["loadAmountPayment"], 10),
                LoadAmountOrder = ReadInt(row["loadAmountOrder"], 25),
                FilterShowProvider = ReadBool(row["filterShowProvider"], true),
                DashboardMoneyLimit = limit,
                FilterShowPerson = ReadBool(row["filterShowPerson"], true),
                AdminEmail = ReadText(row["adminEmail"]),
                NoticeableOnRightSide = ReadBool(row["noticeableOnRightSide"], true),
                SendConfirmationMail = mailMode,
                QuickAmounts = U.SettingsSanitizer.QuickAmountsFromText(ReadText(row["quickAmounts"])),
                Theme = ReadText(row["theme"]),
                Language = ReadText(row["language"]),
                PersonSort = ReadText(row["personSort"]),
                ProviderSort = ReadText(row["providerSort"]),
                Currency = ReadText(row["currency"])
            });
        }

        private static string ReadText(object? value) {
            return value is null or DBNull ? "" : value.ToString() ?? "";
        }

        private static int ReadInt(object? value, int fallback) {
            return int.TryParse(ReadText(value), out int result) ? result : fallback;
        }

        private static bool ReadBool(object? value, bool fallback) {
            if (value is bool b) {
                return b;
            }
            string text = ReadText(value);
            if (text.Length == 0) {
                return fallback;
            }
            return bool.TryParse(text, out bool parsed) ? parsed : text != "0";
        }
        #endregion
    }
}
