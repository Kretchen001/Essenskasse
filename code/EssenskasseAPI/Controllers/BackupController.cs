using EssenskasseAPI.Data;
using EssenskasseAPI.Security;
using Microsoft.AspNetCore.Mvc;
using Modells;
using Modells.User;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/backup")]
    [RequireSession(AdminOnly = true)]
    public class BackupController : ControllerBase {

        #region Config
        private readonly Db _db;

        public BackupController(IConfiguration configuration) {
            _db = new Db(configuration);
        }
        #endregion

        /// <summary>
        /// SessionToken not backup/restore!
        /// </summary>
        /// <returns></returns>
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
            List<Person> persons = DataTableToListOfPerson(_db.Query("SELECT * FROM essenskasse.Person"));
            List<Tuple<Provider, bool>> providers = DataTableToListOfProvider(_db.Query("SELECT * FROM essenskasse.Provider"));
            List<Payment> payments = DataTableToListOfPayments(_db.Query("SELECT * FROM essenskasse.Payment"));
            List<Order> orders = DataTableToListOfOrders(_db.Query("SELECT * FROM essenskasse.Order"));
            List<TransactionProcedure> transactionProcedures = DataTableToListOfTransactionProcedure(_db.Query("SELECT * FROM essenskasse.TransactionProcedure"));
            List<Tuple<Guid, Settings>> settings = DataTableToListOfSettings(_db.Query("SELECT * FROM essenskasse.SettingsUser"));

            //save all as json
            System.IO.File.WriteAllText(dirForBackup + "/person.json", JsonConvert.SerializeObject(persons));
            System.IO.File.WriteAllText(dirForBackup + "/provider.json", JsonConvert.SerializeObject(providers));
            System.IO.File.WriteAllText(dirForBackup + "/payment.json", JsonConvert.SerializeObject(payments));
            System.IO.File.WriteAllText(dirForBackup + "/order.json", JsonConvert.SerializeObject(orders));
            System.IO.File.WriteAllText(dirForBackup + "/transactionProcedure.json", JsonConvert.SerializeObject(transactionProcedures));
            System.IO.File.WriteAllText(dirForBackup + "/settings.json", JsonConvert.SerializeObject(settings));

            return Ok(identifier);
        }

        /// <summary>
        /// SessionToken not backup/restore!
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        [HttpPost, Route("restore")]
        public IActionResult RestoreSpecificBackup([FromHeader] string identifier) {
            // identifier -> "yyyy_MM_dd_HH_mm"
            // Nur dieses Format zulassen, sonst koennte ueber den Header (z. B. "../..") aus dem Backup-Ordner ausgebrochen werden.
            if (!Regex.IsMatch(identifier, @"^\d{4}_\d{2}_\d{2}_\d{2}_\d{2}$")) {
                return BadRequest("Ungültiger Backup-Bezeichner (erwartet: yyyy_MM_dd_HH_mm).");
            }
            string dirForBackup = $"{AppDomain.CurrentDomain.BaseDirectory}backup/{identifier}";

            // Zuerst alle vorhandenen Dateien lesen und auswerten (fehlende Dateien werden ignoriert).
            // Ist eine Datei beschaedigt, ist die Datenbank noch unberuehrt.
            List<Person>? persons = ReadBackupFile<List<Person>>(dirForBackup, "person.json");
            List<Tuple<Provider, bool>>? providers = ReadBackupFile<List<Tuple<Provider, bool>>>(dirForBackup, "provider.json");
            List<Payment>? payments = ReadBackupFile<List<Payment>>(dirForBackup, "payment.json");
            List<Order>? orders = ReadBackupFile<List<Order>>(dirForBackup, "order.json");
            List<TransactionProcedure>? transactionProcedures = ReadBackupFile<List<TransactionProcedure>>(dirForBackup, "transactionProcedure.json");
            List<Tuple<Guid, Settings>>? settings = ReadBackupFile<List<Tuple<Guid, Settings>>>(dirForBackup, "settings.json");

            // Alles in einer Transaktion: bei einem Fehler bleibt die Datenbank im alten Zustand.
            // (TRUNCATE ging dafuer nicht, es bestaetigt sofort; deshalb DELETE.)
            // Fremdschluessel-Pruefung aus, weil die Tabellen komplett ersetzt werden; das Backup
            // selbst ist in sich konsistent.
            using DbScope tx = _db.BeginTransaction(disableForeignKeyChecks: true);

            if (persons is not null) {
                tx.Query("DELETE FROM essenskasse.Person;");
                foreach (Person x in persons) {
                    string queryStringInsert = $"" +
                        $"INSERT INTO essenskasse.Person (Guid, Name, Email, SaldoEuro, SaldoCent) " +
                        $"VALUES (@guid, @name, @email, @euro, @cent);";
                    tx.Query(queryStringInsert,
                        Db.P("@guid", x.Guid),
                        Db.P("@name", Uri.UnescapeDataString(x.Name)),
                        Db.P("@email", Uri.UnescapeDataString(x.Email)),
                        Db.P("@euro", x.Saldo.Euro),
                        Db.P("@cent", x.Saldo.Cent));
                }
            }
            if (providers is not null) {
                tx.Query("DELETE FROM essenskasse.Provider;");
                foreach (Tuple<Provider, bool> x in providers) {
                    string queryStringInsert = $"" +
                        $"INSERT INTO essenskasse.Provider (Guid, Designation, Sichtbar) " +
                        $"VALUES (@guid, @designation, @visible);";
                    tx.Query(queryStringInsert,
                        Db.P("@guid", x.Item1.Guid),
                        Db.P("@designation", x.Item1.Designation),
                        Db.P("@visible", x.Item2));
                }
            }
            if (payments is not null) {
                tx.Query("DELETE FROM essenskasse.Payment;");
                foreach (Payment x in payments) {
                    string queryStringInsert = $"" +
                        $"INSERT INTO essenskasse.Payment (DateTime, AmountEuro, AmountCent, ReferencedPerson, Comment) " +
                        $"VALUES (@dateTime, @euro, @cent, @person, @comment);";
                    tx.Query(queryStringInsert,
                        Db.P("@dateTime", x.DateTime.Ticks),
                        Db.P("@euro", x.Amount.Euro),
                        Db.P("@cent", x.Amount.Cent),
                        Db.P("@person", x.ReferencedPerson),
                        Db.P("@comment", x.Comment));
                }
            }
            if (orders is not null) {
                tx.Query("DELETE FROM essenskasse.Order;");
                foreach (Order x in orders) {
                    string queryStringInsert = $"" +
                        $"INSERT INTO essenskasse.Order (DateTime, AmountEuro, AmountCent, ProviderGuid, ProviderDesignation, ReferencedPerson) " +
                        $"VALUES (@dateTime, @euro, @cent, @providerGuid, @providerDesignation, @person);";
                    tx.Query(queryStringInsert,
                        Db.P("@dateTime", x.DateTime.Ticks),
                        Db.P("@euro", x.Money.Euro),
                        Db.P("@cent", x.Money.Cent),
                        Db.P("@providerGuid", x.Provider.Guid),
                        Db.P("@providerDesignation", x.Provider.Designation),
                        Db.P("@person", x.ReferencedPerson));
                }
            }
            if (transactionProcedures is not null) {
                tx.Query("DELETE FROM essenskasse.TransactionProcedure;");
                foreach (TransactionProcedure x in transactionProcedures) {
                    string queryStringInsert = $"" +
                        $"INSERT INTO essenskasse.TransactionProcedure (DateTime, AmountEuro, AmountCent, Comment) " +
                        $"VALUES (@dateTime, @euro, @cent, @comment);";
                    tx.Query(queryStringInsert,
                        Db.P("@dateTime", x.DateTime.Ticks),
                        Db.P("@euro", x.Amount.Euro),
                        Db.P("@cent", x.Amount.Cent),
                        Db.P("@comment", x.Comment));
                }
            }
            if (settings is not null) {
                tx.Query("DELETE FROM essenskasse.SettingsUser;");
                foreach (Tuple<Guid, Settings> x in settings) {
                    string queryStringInsert = $"" +
                        $"INSERT INTO essenskasse.SettingsUser (UserGuid, isSingleListWishDashboard, " +
                                            $"loadAmountPayment, loadAmountOrder, filterShowProvider, " +
                                            $"dashboardMoneyLimit, filterShowPerson, accessToken, " +
                                            $"adminEmail, noticeableOnRightSide, sendConfirmationMail) " +
                        $"VALUES (@userGuid, @singleList, @loadPayment, @loadOrder, @showProvider, " +
                                            $"@moneyLimit, @showPerson, @accessToken, " +
                                            $"@adminEmail, @rightSide, @sendMail);";
                    tx.Query(queryStringInsert,
                        Db.P("@userGuid", x.Item1),
                        Db.P("@singleList", x.Item2.IsSingleListWishDashboard),
                        Db.P("@loadPayment", x.Item2.LoadAmountPayment),
                        Db.P("@loadOrder", x.Item2.LoadAmountOrder),
                        Db.P("@showProvider", x.Item2.FilterShowProvider),
                        Db.P("@moneyLimit", x.Item2.DashboardMoneyLimit.ToString()),
                        Db.P("@showPerson", x.Item2.FilterShowPerson),
                        Db.P("@accessToken", x.Item2.AccessToken?.ToString() ?? "00000000-0000-0000-0000-000000000000"),
                        Db.P("@adminEmail", x.Item2.AdminEmail),
                        Db.P("@rightSide", x.Item2.NoticeableOnRightSide),
                        Db.P("@sendMail", x.Item2.SendConfirmationMail));
                }
            }
            tx.Commit();
            return Ok();
        }
        #endregion

        #region InternalFunctions
        /// <summary>Liest eine Backup-Datei als JSON. <c>null</c>, wenn die Datei fehlt.</summary>
        private static T? ReadBackupFile<T>(string directory, string fileName) where T : class, new() {
            string path = directory + "/" + fileName;
            if (!System.IO.File.Exists(path)) {
                return null;
            }
            return JsonConvert.DeserializeObject<T>(System.IO.File.ReadAllText(path)) ?? new T();
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

        private static List<Tuple<Guid, Settings>> DataTableToListOfSettings(DataTable dataTable) {
            List<Tuple<Guid, Settings>> settings = [];
            foreach (DataRow row in dataTable.Rows) {
                settings.Add(new Tuple<Guid, Settings>(
                    new Guid(row["UserGuid"].ToString()!),
                    new Settings(
                            bool.Parse(row["isSingleListWishDashboard"].ToString()!),
                            int.Parse(row["loadAmountPayment"].ToString()!),
                            int.Parse(row["loadAmountOrder"].ToString()!),
                            bool.Parse(row["filterShowProvider"].ToString()!),
                            new Money().ToMoney(row["dashboardMoneyLimit"].ToString()!),
                            bool.Parse(row["filterShowPerson"].ToString()!),
                            row["accessToken"].ToString() is null ? null : new Guid(row["accessToken"].ToString()!),
                            row["adminEmail"].ToString()!,
                            bool.Parse(row["noticeableOnRightSide"].ToString()!),
                            bool.Parse(row["sendConfirmationMail"].ToString()!)
                    )
                ));
            }
            return settings;
        }
        #endregion
    }
}
