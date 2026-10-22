using MySql.Data.MySqlClient;
using System.Data;

namespace EssenskasseAPI.Data {

    /// <summary>Zentraler Zugriff auf die MySQL-Datenbank.
    /// Alle Werte werden ausschliesslich ueber Parameter an die Datenbank
    /// uebergeben (kein String-Zusammenbau von Benutzereingaben im SQL).</summary>
    public sealed class Db {

        private readonly string _connectionString;

        /// <summary>Liest den Connection-String <c>ConnectionStrings:essenskasse</c>.</summary>
        /// <param name="configuration">Konfiguration der Anwendung.</param>
        public Db(IConfiguration configuration) {
            _connectionString = configuration["ConnectionStrings:essenskasse"]
                ?? throw new InvalidOperationException("ConnectionStrings:essenskasse ist nicht konfiguriert.");
        }

        /// <summary>Fuehrt ein SQL-Statement mit Parametern aus und liefert das Ergebnis.
        /// Verbindung und Reader werden immer freigegeben, auch bei Fehlern.</summary>
        /// <param name="sql">Statement mit Platzhaltern (z. B. <c>@guid</c>).</param>
        /// <param name="parameters">Parameter, erzeugt mit <see cref="P"/>.</param>
        /// <returns>Ergebnis als DataTable (bei INSERT/UPDATE/DELETE leer).</returns>
        public DataTable Query(string sql, params MySqlParameter[] parameters) {
            using MySqlConnection connection = new MySqlConnection(_connectionString);
            using MySqlCommand cmd = new MySqlCommand(sql, connection);
            if (parameters.Length > 0) {
                cmd.Parameters.AddRange(parameters);
            }
            connection.Open();
            using MySqlDataReader reader = cmd.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        /// <summary>Startet eine Transaktion. Verwendung:
        /// <code>using DbScope tx = db.BeginTransaction(); ... tx.Query(...); tx.Commit();</code></summary>
        /// <param name="disableForeignKeyChecks">Schaltet die Fremdschluessel-Pruefung fuer diese Verbindung aus
        /// (nur fuer das Wiederherstellen eines kompletten Backups gedacht, bei dem Tabellen leer geraeumt und
        /// neu gefuellt werden). Sie wird am Ende wieder eingeschaltet.</param>
        public DbScope BeginTransaction(bool disableForeignKeyChecks = false) {
            return new DbScope(_connectionString, disableForeignKeyChecks);
        }

        /// <summary>Erzeugt einen SQL-Parameter. Guid wird als Text (Format D) uebergeben,
        /// weil die Guid-Spalten als CHAR(36) angelegt sind.</summary>
        /// <param name="name">Name mit @, z. B. <c>@guid</c>.</param>
        /// <param name="value">Wert, <c>null</c> wird zu DBNull.</param>
        public static MySqlParameter P(string name, object? value) {
            object? v = value is Guid g ? g.ToString() : value;
            return new MySqlParameter(name, v ?? DBNull.Value);
        }
    }
}
