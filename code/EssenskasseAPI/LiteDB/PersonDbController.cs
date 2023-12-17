using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {

    /// <summary></summary>
    public class PersonDbController {

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
        private readonly string tableName = "Person";
        #endregion

        #region Function
        /// <summary>Save a person in the database.</summary>
        /// <param name="person">The person, that should be saved.</param>
        /// <returns>Success?</returns>
        public bool SavePerson(Person person) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Person> col = db.GetCollection<Person>(tableName);

                BsonValue? inserted = col.Insert(person);
                col.EnsureIndex(x => x.Guid);

                if (inserted is not null) {
                    return true;
                }
                return false;
            }
        }

        #endregion
    }
}
