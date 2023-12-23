using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent the Money for a database.</summary>
    public class Money {

        /// <summary>long as a pre-decimal number</summary>
        [JsonInclude]
        public long Euro { get; set; } = 0;
        /// <summary>short as a post-decimal number</summary>
        [JsonInclude]
        public short Cent { get; set; } = 0;

        /// <summary></summary>
        public Money() { }

        /// <summary></summary>
        /// <param name="euro"></param>
        /// <param name="cent"></param>
        [JsonConstructor]
        public Money(long euro, short cent) {
            Euro = euro;
            Cent = cent;
        }

        /// <summary>Override the ToString so it returns a valid money String.
        /// For further information see documentation/pap/Money_ToString.puml</summary>
        /// <returns>EURO,CENT€</returns>
        public override string ToString() {
            if (Math.Abs(Cent) < 10) {
                if (Cent < 0) {
                    return ("-" + Math.Abs(Euro).ToString() + ",0" + Math.Abs(Cent).ToString() + "€");
                }
                else {
                    return (Euro.ToString() + ",0" + Math.Abs(Cent).ToString() + "€");
                }
            }
            else {
                if (Cent < 0) {
                    return ("-" + Math.Abs(Euro).ToString() + "," + Math.Abs(Cent).ToString() + "€");
                }
                else if (Euro < 0) {
                    return (Euro.ToString() + "," + Cent.ToString() + "€");
                }
                else {
                    return (Euro.ToString() + "," + Cent.ToString() + "€");
                }
            }
        }
    }
}
