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
        #endregion
    }
}
