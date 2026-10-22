using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent the Money for a database.</summary>
    public class Money {

        /// <summary>long as a pre-decimal number</summary>
        [JsonInclude, JsonPropertyName("Euro"), JsonProperty(nameof(Euro))]
        public long Euro { get; set; } = 0;
        /// <summary>short as a post-decimal number</summary>
        [JsonInclude, JsonPropertyName("Cent"), JsonProperty(nameof(Cent))]
        public short Cent { get; set; } = 0;

        /// <summary>Empty ctor.</summary>
        public Money() { }

        /// <summary></summary>
        /// <param name="euro"></param>
        /// <param name="cent"></param>
        [System.Text.Json.Serialization.JsonConstructor]
        public Money(long euro, short cent) {
            Euro = euro;
            Cent = cent;
        }

        /// <summary>Override the ToString so it returns a valid money String.
        /// For further information see documentation/pap/Money_ToString.puml</summary>
        /// <returns>EURO,CENT €</returns>
        public override string ToString() {
            if (Math.Abs(Cent) < 10) {
                if (Cent < 0) {
                    return ("-" + Math.Abs(Euro).ToString() + ",0" + Math.Abs(Cent).ToString() + " €");
                }
                else {
                    return (Euro.ToString() + ",0" + Math.Abs(Cent).ToString() + " €");
                }
            }
            else {
                if (Cent < 0) {
                    return ("-" + Math.Abs(Euro).ToString() + "," + Math.Abs(Cent).ToString() + " €");
                }
                else if (Euro < 0) {
                    return (Euro.ToString() + "," + Cent.ToString() + " €");
                }
                else {
                    return (Euro.ToString() + "," + Cent.ToString() + " €");
                }
            }
        }

        /// <summary>Convert a given String to a Money.</summary>
        /// <param name="ToStringMoney">String that contain the Money</param>
        /// <returns>A Money Object.</returns>
        public Money ToMoney(string ToStringMoney) {
            // Das Vorzeichen steht in ToString() nur einmal vorn ("-0,05 €"). Es gilt fuer Euro und Cent:
            // sonst ginge es bei 0 Euro verloren und "-8,30 €" waere rechnerisch -7,70 €.
            string text = ToStringMoney.Replace("€", "").Trim();
            bool negative = text.StartsWith('-');
            if (negative) {
                text = text[1..];
            }
            int i = text.IndexOf(',');
            long euro = long.Parse(i < 0 ? text : text[..i], CultureInfo.InvariantCulture);
            short cent = i < 0 ? (short)0 : short.Parse(text[(i + 1)..], CultureInfo.InvariantCulture);
            return negative ? new Money(-euro, (short)-cent) : new Money(euro, cent);
        }

        /// <summary>Der Betrag in Cent (Euro * 100 + Cent). Wirft eine OverflowException statt still zu ueberlaufen.</summary>
        public long ToTotalCents() {
            return checked((Euro * 100) + Cent);
        }

        /// <summary>Erzeugt aus einem Betrag in Cent ein Money; Euro und Cent haben dabei dasselbe Vorzeichen.</summary>
        public static Money FromTotalCents(long totalCents) {
            return new Money(totalCents / 100, (short)(totalCents % 100));
        }

        /// <summary>Is the current Money-value positiv?</summary>
        /// <returns>Positiv or not</returns>
        public bool IsPositiv() {
            if (Cent < 0 || Euro < 0) {
                return false;
            }
            return true;
        }

        /// <summary></summary>
        /// <param name="method">Which mathemtical operation should be used?</param>
        /// <param name="money1">First Parameter (by Subtraction Minued)</param>
        /// <param name="money2">Secount Parameter (by Subtraction Subtrahend)</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822")]
        public Money? CalculateMoney(CalculationOperation? method, Money money1, Money money2) {
            // Es wird ausschliesslich mit ganzen Cent gerechnet (kein double).
            switch (method) {
                case CalculationOperation.Addition: {
                        return FromTotalCents(checked(money1.ToTotalCents() + money2.ToTotalCents()));
                    }
                case CalculationOperation.Substraction: {
                        return FromTotalCents(checked(money1.ToTotalCents() - money2.ToTotalCents()));
                    }
                default: {
                        return null;
                    }
            }
        }

        /// <summary></summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(Money b) {
            if (this.Euro.Equals(b.Euro)) {
                if (this.Cent.Equals(b.Cent)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Return the boolean if the first Money is greater then the secound one.</summary>
        /// <param name="b">Secound Money-Value</param>
        /// <returns>True if a (this) is greater then b</returns>
        public bool IsGreater(Money b) {
            if (this.Euro > b.Euro) {
                return true;
            }
            else if (this.Euro == b.Euro && this.Cent > b.Cent) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
