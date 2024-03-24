using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {
    public class TokenDbController {

        #region Config
        /// <summary>Return the location, where the database is located.</summary>
        private static string SaveDir {
            get {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/token.liteDB")) {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB")) {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB");
                    }
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/token.liteDB").Close();
                }
                return (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/token.liteDB");
            }
        }

        /// <summary>Define the name of the table.</summary>
        private readonly string tableNameAccessToken = "AccessToken";
        #endregion

        #region Function
        public Guid CreateNewAccessToken() {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Tuple<DateTime, Guid>> col = db.GetCollection<Tuple<DateTime, Guid>>(tableNameAccessToken);

                Guid temp = Guid.NewGuid();

                col.Insert(new Tuple<DateTime, Guid>(DateTime.Now, temp));

                return temp;
            }
        }

        public bool CheckAccessToken(Guid accessToken) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Tuple<DateTime, Guid>> col = db.GetCollection<Tuple<DateTime, Guid>>(tableNameAccessToken);

                return col.Query().Where(x => x.Item2.Equals(accessToken)).Exists();
            }
        }
        #endregion
    }
}
