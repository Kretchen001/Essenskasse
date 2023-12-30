using LiteDB;
using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent a order, that is taken.</summary>
    public class Order {

        /// <summary></summary>
        [JsonInclude, BsonId]
        public DateTime DateTime { get; set; }
        /// <summary></summary>
        [JsonInclude]
        public Money Money { get; set; } = new Money();
        /// <summary></summary>
        [JsonInclude]
        public Provider Provider { get; set; } = new Provider();
        /// <summary></summary>
        [JsonInclude]
        public Guid ReferencedPerson{ get; set; } = new Guid();

        /// <summary></summary>
        public Order () { }
        
        /// <summary></summary>
        /// <param name="dateTime"></param>
        /// <param name="money"></param>
        /// <param name="provider"></param>
        /// <param name="referencedPerson"></param>
        [JsonConstructor]
        public Order (DateTime dateTime, Money money, Provider provider, Guid referencedPerson) {
            DateTime = dateTime;
            Money = money;
            Provider = provider;
            ReferencedPerson = referencedPerson;
        }

        /// <summary></summary>
        /// <param name="money"></param>
        /// <param name="provider"></param>
        /// <param name="referencedPerson"></param>
        public Order (Money money, Provider provider, Guid referencedPerson) {
            DateTime = DateTime.Now;
            Money = money;
            Provider = provider;
            ReferencedPerson = referencedPerson;
        }

        /// <summary></summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(Order b) {
            if (this.DateTime.Equals(b.DateTime)) {
                if (this.Money.Equals(b.Money)) {
                    if (this.Provider.Equals(b.Provider)) {
                        if (this.ReferencedPerson.Equals(b.ReferencedPerson)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}