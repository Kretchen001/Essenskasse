using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {

    /// <summary></summary>
    public class OrderDbController {

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
        private readonly string tableName = "Order";
        #endregion

        #region Function
        /// <summary></summary>
        /// <param name="newOrder"></param>
        /// <returns></returns>
        public bool NewOrder(Order newOrder) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Order> col = db.GetCollection<Order>(tableName);

                BsonValue? inserted = col.Insert(newOrder);

                if (inserted is not null) {
                    return true;
                }
                return false;
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        public List<Order>? RequestAllOrders() {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Order> col = db.GetCollection<Order>(tableName);

                return (col.FindAll().OrderByDescending(x => x.DateTime).ToList() ?? null);
            }
        }

        /// <summary></summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<Order>? RequestAmountOfOrders(int amount) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Order> col = db.GetCollection<Order>(tableName);

                return (col.FindAll().OrderByDescending(x => x.DateTime).Take(amount).ToList() ?? null);
            }
        }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<Order>? OrdersFromOnePersonPerGuid(Guid guid, int? amount = 0) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Order> col = db.GetCollection<Order>(tableName);
                List<Order>? matchingPayments = new List<Order>();

                IEnumerator<Order> enumerator = col.FindAll().GetEnumerator();
                while (enumerator.MoveNext()) {
                    Order x = enumerator.Current;
                    if (x.ReferencedPerson.Equals(guid)) {
                        matchingPayments.Add(new Order(
                            dateTime: x.DateTime,
                            money: x.Money,
                            provider: x.Provider,
                            referencedPerson: x.ReferencedPerson
                        ));
                    }
                }
                enumerator.Dispose();

                if (amount == 0) {
                    return matchingPayments;
                }
                else {
                    return matchingPayments.OrderByDescending(x => x.DateTime).Take(amount ?? 0).ToList();
                }
            }
        }

        /// <summary></summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public Order? OrderAtDateTime(DateTime date) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Order> col = db.GetCollection<Order>(tableName);

                return col.FindOne(x => x.DateTime.Equals(date));
            }
        }


        /// <summary></summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool UpdateOrder(Order order) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Order> col = db.GetCollection<Order>(tableName);

                bool updated = col.Update(order.DateTime, order);

                if (updated) {
                    return true;
                }
                return false;
            }
        }

        /// <summary></summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool RemoveOrder(Order order) {
            using (LiteDatabase database = new LiteDatabase(saveDir)) {
                ILiteCollection<Payment> col = database.GetCollection<Payment>();

                return col.Delete(order.DateTime);
            }
        }
        #endregion
    }
}
