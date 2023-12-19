using LiteDB;
using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent a registered person.</summary>
    public class Person {

        /// <summary></summary>
        [JsonInclude, BsonId]
        public Guid Guid { get; set; }
        /// <summary></summary>
        [JsonInclude]
        public string Name { get; set; } = "";
        /// <summary></summary>
        [JsonInclude]
        public string Email { get; set; } = "";
        /// <summary></summary>
        [JsonInclude]
        public Money Saldo = new Money();

        /// <summary></summary>
        public Person() { }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="money"></param>
        public Person(Guid guid, string name, string? email, Money money) {
            Guid = guid;
            Name = name;
            Email = email ?? "";
            Saldo = money;
        }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        [JsonConstructor]
        public Person(Guid guid, string name, string? email) {
            Guid = guid;
            Name = name;
            Email = email ?? "";
        }

        /// <summary></summary>
        /// <param name="name"></param>
        public Person(string? name) {
            Guid = Guid.NewGuid();
            Name = name ?? ""; // if name is null then schreibe ""
        }

        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public Person(string? name, string? email) {
            Guid = Guid.NewGuid();
            Name = name ?? ""; // if name is null then schreibe ""
            Email = email ?? "";
        }
    }
}
