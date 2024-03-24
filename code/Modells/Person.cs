using LiteDB;
using System;
using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent a registered person.</summary>
    public class Person {

        /// <summary></summary>
        [JsonInclude, BsonId]
        public Guid Guid { get; set; } = new Guid();
        /// <summary></summary>
        [JsonInclude]
        public string Name { get; set; } = "";
        /// <summary></summary>
        [JsonInclude]
        public string Email { get; set; } = "";
        /// <summary></summary>
        [JsonInclude]
        public Money Saldo { get; set; } = new Money();

        /// <summary></summary>
        public Person() { }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="money"></param>
        [JsonConstructor]
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

        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="money"></param>
        public Person(string name, string? email, Money money) {
            Guid = Guid.NewGuid();
            Name = name;
            Email = email ?? "";
            Saldo = money;
        }

        /// <summary></summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(Person b) {
            if (this.Guid.Equals(b.Guid)) {
                if (this.Name.Equals(b.Name)) {
                    if (this.Email.Equals(b.Email)) {
                        if (this.Saldo.Equals(b.Saldo)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
