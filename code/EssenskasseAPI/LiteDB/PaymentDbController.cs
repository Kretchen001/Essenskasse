using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {

    /// <summary></summary>
    public class PaymentDbController {

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
        private readonly string tableName = "Payment";
        #endregion

        #region Function
        /// <summary></summary>
        /// <param name="newPayment"></param>
        /// <returns></returns>
        public bool NewPayment(Payment newPayment) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                BsonValue? inserted = col.Insert(newPayment);

                if (inserted is not null) {
                    return true;
                }
                return false;
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        public List<Payment>? RequestAllPayments() {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                return (col.FindAll().OrderByDescending(x => x.DateTime).ToList() ?? null);
            }
        }

        /// <summary></summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<Payment> RequestAmountOfPayments(int amount) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                return col.FindAll().OrderByDescending(x => x.DateTime).Take(amount).ToList();
            }
        }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<Payment> PaymentsFromOnePersonPerGuid(Guid guid, int? amount) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);
                List<Payment> matchingPayments = [];

                IEnumerator<Payment> enumerator = col.FindAll().GetEnumerator();
                while (enumerator.MoveNext()) {
                    Payment x = enumerator.Current;
                    if (x.ReferencedPerson.Equals(guid)) {
                        matchingPayments.Add(x);
                    }
                }
                enumerator.Dispose();

                if (amount is null) {
                    return matchingPayments.OrderByDescending(x => x.DateTime).ToList();
                }
                else {
                    return matchingPayments.OrderByDescending(x => x.DateTime).Take(amount ?? 1).ToList();
                }
            }
        }

        /// <summary></summary>
        /// <param name="date"></param>
        /// <returns>the payment or null if not found</returns>
        public Payment? PaymentAtDateTime(DateTime date) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                return col.FindOne(x => x.DateTime.Equals(date));
            }
        }

        /// <summary></summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public bool UpdatePayment(Payment payment) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableName);

                bool updated = col.Update(payment.DateTime, payment);

                if (updated) {
                    return true;
                }
                return false;
            }
        }

        /// <summary></summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public bool RemovePayment(Payment payment) {
            using (LiteDatabase database = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = database.GetCollection<Payment>();

                return col.Delete(payment.DateTime);
            }
        }
        #endregion
    }
}
