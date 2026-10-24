using MySql.Data.MySqlClient;
using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EssenskasseAPI.Data {

    /// <summary>Spielt beim Start der API alle noch nicht angewendeten Update-Skripte (<c>MySqlScripts/update_vX-Y-Z_zu_A-B-C.sql</c>)
    /// in die Datenbank ein. Welche Skripte schon gelaufen sind, steht in der Tabelle <c>essenskasse.SchemaMigration</c>.
    /// Die Skripte sind in die API eingebettet, ein manuelles Ausfuehren ist nicht noetig (aber weiterhin moeglich:
    /// sie sind so geschrieben, dass sie beliebig oft laufen koennen).</summary>
    public sealed class DatabaseMigrator {

        private const string ResourcePrefix = "Migrations/";
        private const string LockName = "essenskasse_migrations";
        private static readonly Regex NamePattern =
            new(@"^update_v(\d+)-(\d+)-(\d+)_zu_(\d+)-(\d+)-(\d+)$", RegexOptions.Compiled);

        private readonly string _connectionString;

        /// <param name="connectionString">Verbindung zur Datenbank; fuer die Skripte werden Benutzervariablen (<c>SET @x</c>) erlaubt.</param>
        public DatabaseMigrator(string connectionString) {
            MySqlConnectionStringBuilder builder = new(connectionString) { AllowUserVariables = true };
            _connectionString = builder.ConnectionString;
        }

        #region Reihenfolge
        /// <summary>Sortiert Skriptnamen nach Version (von, dann nach), also v1-9-0 vor v1-10-0.
        /// Ein Name, der nicht dem Muster <c>update_vX-Y-Z_zu_A-B-C</c> entspricht, ist ein Fehler.</summary>
        public static IReadOnlyList<string> Order(IEnumerable<string> ids) {
            return ids
                .Select(id => (Id: id, Key: ParseVersions(id)))
                .OrderBy(x => x.Key.From)
                .ThenBy(x => x.Key.To)
                .Select(x => x.Id)
                .ToList();
        }

        /// <summary>Die Skripte aus <paramref name="all"/>, die noch nicht in <paramref name="applied"/> stehen, in Ausfuehrungsreihenfolge.</summary>
        public static IReadOnlyList<string> Pending(IEnumerable<string> all, IEnumerable<string> applied) {
            HashSet<string> done = new(applied, StringComparer.Ordinal);
            return Order(all).Where(id => !done.Contains(id)).ToList();
        }

        private static (Version From, Version To) ParseVersions(string id) {
            Match match = NamePattern.Match(id);
            if (!match.Success) {
                throw new InvalidOperationException($"Ungueltiger Name eines Update-Skripts: '{id}' (erwartet: update_v1-2-0_zu_1-3-0).");
            }
            int[] n = match.Groups.Cast<Group>().Skip(1).Select(g => int.Parse(g.Value)).ToArray();
            return (new Version(n[0], n[1], n[2]), new Version(n[3], n[4], n[5]));
        }
        #endregion

        #region Ausfuehren
        /// <summary>Wendet alle fehlenden Skripte an. Wartet dafuer bis zu <paramref name="maxWaitSeconds"/> Sekunden auf die Datenbank
        /// (beim gemeinsamen Start per docker-compose ist sie evtl. noch nicht bereit). Scheitert ein Skript, wird eine
        /// Ausnahme geworfen und es wird nichts weiter angewendet; das Skript gilt dann nicht als angewendet.</summary>
        public void Run(int maxWaitSeconds = 60) {
            Dictionary<string, string> scripts = ReadEmbeddedScripts();
            using MySqlConnection connection = OpenWithRetry(maxWaitSeconds);

            // Nur eine API-Instanz gleichzeitig migrieren (die Sperre faellt mit der Verbindung weg).
            using (MySqlCommand lockCommand = new("SELECT GET_LOCK(@name, 120);", connection)) {
                lockCommand.Parameters.AddWithValue("@name", LockName);
                lockCommand.ExecuteScalar();
            }

            using (MySqlCommand create = new(
                "CREATE TABLE IF NOT EXISTS essenskasse.SchemaMigration (" +
                "Id VARCHAR(190) NOT NULL PRIMARY KEY, AppliedAt DATETIME NOT NULL);", connection)) {
                create.ExecuteNonQuery();
            }

            List<string> applied = [];
            using (MySqlCommand select = new("SELECT Id FROM essenskasse.SchemaMigration;", connection))
            using (MySqlDataReader reader = select.ExecuteReader()) {
                while (reader.Read()) {
                    applied.Add(reader.GetString(0));
                }
            }

            IReadOnlyList<string> pending = Pending(scripts.Keys, applied);
            if (pending.Count == 0) {
                Log.Information("Database is up to date ({Count} update scripts applied).", applied.Count);
                return;
            }
            foreach (string id in pending) {
                Log.Information("Applying database update {Id}...", id);
                try {
                    new MySqlScript(connection, scripts[id]).Execute();
                }
                catch (Exception ex) {
                    Log.Fatal(ex, "Database update {Id} failed. It is not marked as applied.", id);
                    throw;
                }
                using MySqlCommand mark = new("INSERT INTO essenskasse.SchemaMigration (Id, AppliedAt) VALUES (@id, UTC_TIMESTAMP());", connection);
                mark.Parameters.AddWithValue("@id", id);
                mark.ExecuteNonQuery();
                Log.Information("Database update {Id} applied.", id);
            }
        }

        private MySqlConnection OpenWithRetry(int maxWaitSeconds) {
            DateTime deadline = DateTime.UtcNow.AddSeconds(maxWaitSeconds);
            while (true) {
                MySqlConnection connection = new(_connectionString);
                try {
                    connection.Open();
                    return connection;
                }
                catch (MySqlException ex) {
                    connection.Dispose();
                    if (DateTime.UtcNow >= deadline) {
                        throw;
                    }
                    Log.Warning("Database not reachable yet ({Message}), retrying...", ex.Message);
                    Thread.Sleep(2000);
                }
            }
        }

        private static Dictionary<string, string> ReadEmbeddedScripts() {
            Assembly assembly = typeof(DatabaseMigrator).Assembly;
            Dictionary<string, string> scripts = new(StringComparer.Ordinal);
            foreach (string resource in assembly.GetManifestResourceNames().Where(x => x.StartsWith(ResourcePrefix, StringComparison.Ordinal))) {
                using Stream stream = assembly.GetManifestResourceStream(resource)!;
                using StreamReader reader = new(stream);
                string id = Path.GetFileNameWithoutExtension(resource);
                scripts[id] = reader.ReadToEnd();
            }
            return scripts;
        }
        #endregion
    }
}
