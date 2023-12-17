using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {

    /// <summary></summary>
    public class ProviderDbController {

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
        private readonly string tableName = "Provider";
        #endregion

        #region Function
        /// <summary>Save the provider in the database.</summary>
        /// <param name="Provider">Provider that should be saved.</param>
        /// <returns>Success?</returns>
        public bool SaveProvider(Provider Provider) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Provider> col = db.GetCollection<Provider>(tableName);

                BsonValue? inserted = col.Insert(Provider);
                col.EnsureIndex(x => x.Guid);

                if (inserted is not null) {
                    return true;
                }
                return false;
            }
        }

        /// <summary>Request all provider as list, that can be found in the database.</summary>
        /// <returns>List of provider</returns>
        public List<Provider> RequestProvider() {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Provider> col = db.GetCollection<Provider>(tableName);

                return col.FindAll().ToList();
            }
        }

        /// <summary>Look in the database, if the provider exist</summary>
        /// <param name="guid">Guid of the requested provider</param>
        /// <param name="name">Designator of the requested provider</param>
        /// <returns>Provider if exist, else null.</returns>
        public Provider? ExistProvider(Guid? guid, string? name) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Provider> col = db.GetCollection<Provider>(tableName);

                Provider? result = col.Find(x => x.Guid == guid).SingleOrDefault();
                if (result is null) {
                    name ??= ""; // if name is null then ""
                    result = col.Find(x => x.Designation.Equals(name)).SingleOrDefault();
                }
                return result;
            }
        }

        /// <summary>Save the changes of a single provider.</summary>
        /// <param name="provider">New Provider, but the Guid will be the same, so it is the identifier.</param>
        /// <returns>Success?</returns>
        public bool SaveChangesAtProvider(Provider provider) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Provider> col = db.GetCollection<Provider>(tableName);

                bool updated = col.Update(provider.Guid, provider);

                if (updated) {
                    return true;
                }
                return false;
            }
        }

        /// <summary>Remove one provider from the database.</summary>
        /// <param name="Provider"></param>
        /// <returns>Success?</returns>
        public bool RemoveProvider(Provider Provider) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Provider> col = db.GetCollection<Provider>(tableName);

                return col.Delete(Provider.Guid);
            }
        }
        #endregion
    }
}
