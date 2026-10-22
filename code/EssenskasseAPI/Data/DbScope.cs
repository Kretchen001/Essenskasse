using MySql.Data.MySqlClient;
using System.Data;

namespace EssenskasseAPI.Data {

    /// <summary>Eine Datenbank-Transaktion ueber mehrere Statements. Alle Statements laufen auf einer
    /// Verbindung. Ohne <see cref="Commit"/> macht <see cref="Dispose"/> alles rueckgaengig, also auch
    /// bei einer Exception oder einem vorzeitigen <c>return</c> innerhalb von <c>using</c>.
    /// Mit <c>SELECT ... FOR UPDATE</c> werden Zeilen bis zum Ende der Transaktion gesperrt,
    /// damit gleichzeitige Buchungen auf dieselbe Person nicht aneinander vorbei rechnen.</summary>
    public sealed class DbScope : IDisposable {

        private readonly MySqlConnection _connection;
        private readonly MySqlTransaction _transaction;
        private readonly bool _foreignKeyChecksDisabled;

        internal DbScope(string connectionString, bool disableForeignKeyChecks = false) {
            _connection = new MySqlConnection(connectionString);
            try {
                _connection.Open();
                if (disableForeignKeyChecks) {
                    // Sitzungsvariable, gilt nur fuer diese Verbindung; wird in Dispose zurueckgesetzt
                    // (Verbindungen kommen in einen Pool und wuerden die Einstellung sonst behalten).
                    using MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS=0;", _connection);
                    cmd.ExecuteNonQuery();
                    _foreignKeyChecksDisabled = true;
                }
                _transaction = _connection.BeginTransaction();
            }
            catch {
                _connection.Dispose();
                throw;
            }
        }

        /// <summary>Fuehrt ein Statement innerhalb der Transaktion aus (siehe <see cref="Db.Query"/>).</summary>
        public DataTable Query(string sql, params MySqlParameter[] parameters) {
            using MySqlCommand cmd = new MySqlCommand(sql, _connection, _transaction);
            if (parameters.Length > 0) {
                cmd.Parameters.AddRange(parameters);
            }
            using MySqlDataReader reader = cmd.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        /// <summary>Uebernimmt alle bisherigen Aenderungen dauerhaft.</summary>
        public void Commit() {
            _transaction.Commit();
        }

        /// <summary>Beendet die Transaktion; nicht bestaetigte Aenderungen werden zurueckgerollt.</summary>
        public void Dispose() {
            _transaction.Dispose();
            if (_foreignKeyChecksDisabled) {
                try {
                    using MySqlCommand cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS=1;", _connection);
                    cmd.ExecuteNonQuery();
                }
                catch {
                    // Ist die Verbindung kaputt, wird sie nicht in den Pool zurueckgegeben.
                    MySqlConnection.ClearPool(_connection);
                }
            }
            _connection.Dispose();
        }
    }
}
