using LiteDB;
using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent a Payment for the database</summary>
    public class Payment {

        /// <summary>When the payment took place.</summary>
        [JsonInclude, BsonId]
        public DateTime DateTime { get; set; } = new DateTime();
        /// <summary>How much money is added to the chest.</summary>
        [JsonInclude]
        public Money Amount { get; set; } = new Money();
        /// <summary>Which person is associated with this payment.</summary>
        [JsonInclude]
        public Guid ReferencedPerson { get; set; } = new Guid();
        /// <summary></summary>
        [JsonInclude]
        public string Comment { get; set; } = default!;

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
        /// <param name="amount"></param>
        /// <param name="referencedPerson"></param>
        /// <param name="comment"></param>
        public Payment(Money amount, Guid referencedPerson, string comment) {
            DateTime = DateTime.Now;
            Amount = amount;
            ReferencedPerson = referencedPerson;
            Comment = comment;
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

        /// <summary></summary>
        /// <param name="dateTime"></param>
        /// <param name="amount"></param>
        /// <param name="referencedPerson"></param>
        /// <param name="comment"></param>
        [JsonConstructor]
        public Payment(DateTime dateTime, Money amount, Guid referencedPerson, string comment) {
            DateTime = dateTime;
            Amount = amount;
            ReferencedPerson = referencedPerson;
            Comment = comment;
        }

        /// <summary></summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(Payment b) {
            if (this.DateTime.Equals(b.DateTime)) {
                if (this.Amount.Equals(b.Amount)) {
                    if (this.ReferencedPerson.Equals(b.ReferencedPerson)) {
                        if(this.Comment.Equals(b.Comment)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}