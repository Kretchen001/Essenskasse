using LiteDB;
using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Money for the "coffee cash box".</summary>
    public class TransactionProcedure {

        /// <summary>When the transaction took place.</summary>
        [JsonInclude, BsonId]
        public DateTime DateTime { get; set; }
        /// <summary>How much money is added to the chest.</summary>
        [JsonInclude]
        public Money Amount { get; set; } = new Money();
        /// <summary>Comment on the transaction.</summary>
        [JsonInclude]
        public string Comment { get; set; } = default!;

        public TransactionProcedure() {
        }
    }
}
