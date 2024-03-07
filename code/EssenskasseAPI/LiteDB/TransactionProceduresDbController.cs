using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {

    public class TransactionProceduresDbController {

        #region Config
        /// <summary>Return the location, where the database is located.</summary>
        private static string SaveDir {
            get {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB")) {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB")) {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB");
                    }
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB").Close();
                }
                return (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB");
            }
        }

        /// <summary>Define the name of the table.</summary>
        private readonly string tableName = "TransactionProcedures";
        #endregion

        #region Function
        /// <summary></summary>
        /// <returns></returns>
        public List<TransactionProcedure> GetAll() {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<TransactionProcedure> col = db.GetCollection<TransactionProcedure>(tableName);

                return col.Query().ToList();
            }
        }

        /// <summary></summary>
        /// <param name="newTransaction"></param>
        /// <returns></returns>
        public bool NewTransactionProcedure(TransactionProcedure newTransaction) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<TransactionProcedure> col = db.GetCollection<TransactionProcedure>(tableName);

                BsonValue? inserted = col.Insert(newTransaction);

                if (inserted is not null) {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}