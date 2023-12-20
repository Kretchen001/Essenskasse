using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent a Payment for the database</summary>
    public class Payment {

        /// <summary>When the payment took place.</summary>
        [JsonInclude]
        public DateTime DateTime { get; set; }
        /// <summary>How much money is added to the chest.</summary>
        [JsonInclude]
        public Money Amount { get; set; } = new Money();
        /// <summary>Which person is associated with this payment.</summary>
        [JsonInclude]
        public Guid ReferencedPerson { get; set; }

        /// <summary></summary>
        public Payment() { }

        /// <summary></summary>
        /// <param name="amount"></param>
        /// <param name="referencedPerson"></param>
        public Payment(Money amount, Guid referencedPerson) {
            DateTime = DateTime.Now;
            Amount = amount;
            ReferencedPerson = referencedPerson;
        }

        /// <summary></summary>
        /// <param name="dateTime"></param>
        /// <param name="amount"></param>
        /// <param name="referencedPerson"></param>
        [JsonConstructor]
        public Payment(DateTime dateTime, Money amount, Guid referencedPerson) {
            DateTime = dateTime;
            Amount = amount;
            ReferencedPerson = referencedPerson;
        }
    }
}