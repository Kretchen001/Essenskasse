using LiteDB;
using Modells;
using System.Collections.Generic;
using System.Linq;

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
        /// <summary></summary>
        /// <param name="newPayment"></param>
        /// <returns></returns>
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

        /// <summary></summary>
        /// <returns></returns>
        public List<Payment>? RequestAllPayments() {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                return (col.FindAll().OrderByDescending(x => x.DateTime).ToList() ?? null);
            }
        }

        /// <summary></summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<Payment>? RequestAmountOfPayments(int amount) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                return (col.FindAll().OrderByDescending(x => x.DateTime).Take(amount).ToList() ?? null);
            }
        }
        #endregion
    }
}
