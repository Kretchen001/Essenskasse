using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {

    /// <summary></summary>
    public class PaymentDbController {

        #region Config
        /// <summary>Return the location, where the database is located.</summary>
        private string saveDir {
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
        private readonly string tableName = "Payment";
        #endregion

        #region Function
        public bool NewPayment(Payment newPayment) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                BsonValue? inserted = col.Insert(newPayment);
                //col.EnsureIndex(x => x.Guid);

                if (inserted is not null) {
                    return true;
                }
                return false;
            }
        }
        #endregion
    }
}
