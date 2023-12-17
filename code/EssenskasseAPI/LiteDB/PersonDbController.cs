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

        /// <summary>Update a person in the database.</summary>
        /// <param name="person">Information to update person.</param>
        /// <returns>Success?</returns>
        public bool UpdatePerson(Person person) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Person> col = db.GetCollection<Person>(tableName);

                bool updated = col.Update(person.Guid, person);

                if (updated) {
                    return true;
                }
                return false;
            }
        }

        /// <summary>Request all person, that are saved in the database.</summary>
        /// <returns>List of all person.</returns>
        public List<Person> RequestAllPerson() {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Person> col = db.GetCollection<Person>(tableName);

                return col.FindAll().ToList();
            }
        }

        /// <summary>Search a person with the Guid.</summary>
        /// <param name="guid">Guid of the search person</param>
        /// <returns>Person if it is there</returns>
        public Person? PersonPerGuid(Guid guid) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Person> col = db.GetCollection<Person>(tableName);
                Person? result = col.Query().Where(x => x.Guid.Equals(guid)).SingleOrDefault();
                
                return result;
            }
        }

        /// <summary>Search a person with the name.</summary>
        /// <param name="name">Name of the search person</param>
        /// <returns>Person if it is there.</returns>
        public Person? PersonPerName(string name) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Person> col = db.GetCollection<Person>(tableName);
                Person? result = col.Query().Where(x => x.Name.Equals(name)).SingleOrDefault();

                return result;
            }
        }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Person? ExistPerson(Guid? guid, string? name) {
            if (guid is null) {
                return null;
            }
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Person> col = db.GetCollection<Person>(tableName);

                Person? result = col.Find(x => x.Guid == guid).SingleOrDefault();
                if (result is null) {
                    name ??= ""; // if name is null then ""
                    result = col.Find(x => x.Name.Equals(name)).SingleOrDefault();
                }
                return result;
            }
        }

        /// <summary>Remove one person from the database.</summary>
        /// <param name="person"></param>
        /// <returns>Success?</returns>
        public bool RemovePerson(Person person) {
            using (LiteDatabase db = new LiteDatabase(saveDir)) {
                ILiteCollection<Person> col = db.GetCollection<Person>(tableName);

                return col.Delete(person.Guid);
            }
        }
        #endregion
    }
}
