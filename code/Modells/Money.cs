using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent the Money for a database.</summary>
    public class Money {

        /// <summary>ulong as a pre-decimal number</summary>
        [JsonInclude]
        public ulong Euro { get; set; } = 0;
        /// <summary>ushort as a post-decimal number</summary>
        [JsonInclude]
        public ushort Cent { get; set; } = 0;

        /// <summary></summary>
        public Money() { }

        /// <summary></summary>
        /// <param name="euro"></param>
        /// <param name="cent"></param>
        [JsonConstructor]
        public Money(ulong euro, ushort cent) {
            Euro = euro;
            Cent = cent;
        }

        /// <summary>Override the ToString so it returns a valid String</summary>
        /// <returns>EURO,CENT€</returns>
        public override string ToString() {
            if (Cent < 10) {
                return (Euro.ToString() + ",0" + Cent.ToString() + "€");
            }
            else {
                return (Euro.ToString() + "," + Cent.ToString() + "€");
            }
        }
    }
}
