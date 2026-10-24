using EssenskasseAPI.Data;
using EssenskasseAPI.Security;
using Microsoft.AspNetCore.Mvc;
using Modells;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/provider")]
    [RequireSession]
    public class ProviderController : ControllerBase {

        #region Config
        private readonly Db _db;

        public ProviderController(IConfiguration configuration) {
            _db = new Db(configuration);
        }
        #endregion

        #region Controller
        [HttpGet, Route("requestAll")]
        public IActionResult GetAllProvider() {
            using (Operation.Time("TaskDuration")) {
                Log.Information("GetAllProvider called");

                string queryString = $"" +
                    $"SELECT * " +
                    $"FROM essenskasse.Provider " +
                    $"WHERE Sichtbar=true;";

                DataTable dataTableProvider = _db.Query(queryString);
                List<Provider> provider = DataTableToListOfProvider(dataTableProvider);

                if (provider.Count != 0) {
                    Log.Information(provider.Count + " Provider requested");
                    return Ok(JsonConvert.SerializeObject(provider));
                }
                else {
                    Log.Warning("No Data response (GetAllProvider)");
                    return NoContent();
                }
            }
        }

        /// <summary>Anzahl, Summe und letzte Bestellung je Anbieter (nur Anbieter mit mindestens einer Bestellung).</summary>
        [HttpGet, Route("statistics")]
        public IActionResult GetProviderStatistics() {
            using (Operation.Time("GetProviderStatistics")) {
                Log.Information("GetProviderStatistics called");
                DataTable table = _db.Query($"" +
                    $"SELECT ProviderGuid, COUNT(*) AS OrderCount, " +
                    $"SUM(AmountEuro * 100 + AmountCent) AS TotalCents, MAX(`DateTime`) AS LastOrder " +
                    $"FROM essenskasse.`Order` " +
                    $"GROUP BY ProviderGuid;");

                List<ProviderStatistics> statistics = [];
                foreach (DataRow row in table.Rows) {
                    if (row["ProviderGuid"] is DBNull) {
                        continue;
                    }
                    statistics.Add(new ProviderStatistics(
                        new Guid(row["ProviderGuid"].ToString()!),
                        Convert.ToInt32(row["OrderCount"]),
                        Convert.ToInt64(row["TotalCents"]),
                        row["LastOrder"] is DBNull ? null : new DateTime(Convert.ToInt64(row["LastOrder"])))); // DateTime ist als Ticks (BIGINT) gespeichert
                }
                return Ok(JsonConvert.SerializeObject(statistics));
            }
        }

        [HttpGet, Route("testExist")]
        public IActionResult CheckExistProvider([FromHeader] Guid? guid, [FromHeader] string? designator) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("CheckExistProvider called");

                string queryString = "";
                if (guid is not null) {
                    queryString = $"" +
                       $"SELECT * " +
                       $"FROM essenskasse.Provider " +
                       $"WHERE Guid=@guid;";
                }
                if (designator is not null) {
                    queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Provider " +
                        $"WHERE Guid=@guid OR Designation=@designator;";
                }
                if (guid is null && designator is null) {
                    Log.Warning("Both Headers null!");
                    return BadRequest("Beide Header null!");
                }

                DataTable dataTableProvider = _db.Query(queryString,
                    Db.P("@guid", guid),
                    Db.P("@designator", designator));
                List<Provider> provider = DataTableToListOfProvider(dataTableProvider);

                if (provider.Count == 1) {
                    if (provider[0].Designation != designator) {
                        Log.Information($"{guid} with other {provider[0].Designation} found and not {designator}");
                        return Conflict("Guid existiert, aber mit anderer Bezeichnung!");
                    }
                    Log.Information("Provider exist");
                    return Ok(JsonConvert.SerializeObject(provider));
                }
                else {
                    Log.Warning(guid.ToString() + " not found. Or designation not found");
                    return NoContent();
                }
            }
        }

        [HttpPost, Route("new")]
        public IActionResult AddProvider([FromHeader] string? name) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("AddProvider called");
                if (name is not null) {
                    name = Uri.UnescapeDataString(name);
                    Provider newProvider = new Provider(name);

                    string queryString = $"" +
                        $"INSERT INTO essenskasse.Provider (Guid, Designation) " +
                        $"VALUES (@guid, @designation);";

                    try {
                        _db.Query(queryString,
                            Db.P("@guid", newProvider.Guid),
                            Db.P("@designation", newProvider.Designation));

                        Log.Information($"New provider {newProvider.Designation} and {newProvider.Guid}");
                        return Ok(JsonConvert.SerializeObject(newProvider.Guid));
                    }
                    catch (Exception ex) {
                        Log.Error($"AddProvider failed - {ex.Message} - {ex.StackTrace}");
                        return BadRequest("Provider konnte nicht eingefügt werden. Bitte Admin benachrichten.");
                    }
                }
                else {
                    Log.Warning("Nothing to add");
                    return NoContent();
                }
            }
        }

        [HttpPut, Route("update")]
        public IActionResult ChangeProvider([FromHeader] Guid? guid, [FromHeader] string? designation) {
            using (Operation.Time("TaskDuration")) {
                Log.Information("ChangeProvider called");
                if (guid is not null) {
                    string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Provider " +
                        $"WHERE Guid=@guid;";

                    DataTable dataTableProvider = _db.Query(queryString, Db.P("@guid", guid));
                    List<Provider> provider = DataTableToListOfProvider(dataTableProvider);

                    if (provider.Count == 1) {
                        string oldDesignation = provider[0].Designation;

                        try {
                            if (designation is not null) {
                                string queryUpdateString = $"" +
                                    $"UPDATE essenskasse.Provider " +
                                    $"SET Designation=@designation " +
                                    $"WHERE Guid=@guid;";
                                _db.Query(queryUpdateString,
                                    Db.P("@designation", Uri.UnescapeDataString(designation)),
                                    Db.P("@guid", guid));
                            }
                            Log.Information($"Provider successfully updated from {oldDesignation} to {designation}");
                            return Ok();
                        }
                        catch (Exception ex) {
                            Log.Error($"UpdateProvider failed - {ex.Message} - {ex.StackTrace}");
                            return BadRequest("Änderungen konnten nicht übernommen werden. Bitte beim Admin melden.");
                        }
                    }
                    else {
                        Log.Warning($"Guid has no entry (provider) - {guid}");
                        return NotFound("Provider konnte nicht gefunden werden");
                    }
                }
                else {
                    Log.Warning("No Guid there");
                    return NoContent();
                }
            }
        }

        [HttpDelete, Route("remove")]
        public IActionResult RemoveProvider([FromHeader] Guid? guid,
                                                [FromHeader] bool per = false) {
            using (Operation.Time("RemoveProvider")) {
                Log.Information($"RemoveProvider called with per={per}");
                if (guid is not null) {
                    string queryString = $"" +
                        $"SELECT * " +
                        $"FROM essenskasse.Provider " +
                        $"WHERE Guid=@guid;";
                    DataTable dataTable = _db.Query(queryString, Db.P("@guid", guid));
                    List<Provider> provider = DataTableToListOfProvider(dataTable);

                    if (provider.Count == 1) {
                        if (per == false) {
                            string queryStringInvisible = $"" +
                                $"UPDATE essenskasse.Provider " +
                                $"SET Sichtbar=false " +
                                $"WHERE Guid=@guid;";
                            _db.Query(queryStringInvisible, Db.P("@guid", guid));
                            Log.Information($"{provider[0].Designation} with {provider[0].Guid} successfully invisible set");
                            return Ok(JsonConvert.SerializeObject(provider[0]));
                        }
                        else {
                            string queryStringDelete = $"" +
                                $"DELETE " +
                                $"FROM essenskasse.Provider " +
                                $"WHERE Guid=@guid;";
                            try {
                                _db.Query(queryStringDelete, Db.P("@guid", guid));
                            }
                            catch (MySqlException ex) when (ex.Number == 1451) { // Fremdschluessel: es gibt noch Bestellungen
                                Log.Warning($"{provider[0].Guid} has Orders and can not be removed permanently");
                                return Conflict("Der Anbieter hat Bestellungen und kann nicht endgültig gelöscht werden. Bitte stattdessen ausblenden (per=false).");
                            }
                            Log.Information($"{provider[0].Designation} with {provider[0].Guid} successfully removed");
                            return Ok(JsonConvert.SerializeObject(provider[0]));
                        }
                    }
                    Log.Warning($"{guid} not found");
                    return NotFound("Person konnte nicht gefunden werden!");
                }
                else {
                    Log.Warning("Nothing to remove");
                    return NoContent();
                }
            }
        }
        #endregion

        #region InternalFunctions
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
        #endregion
    }
}

