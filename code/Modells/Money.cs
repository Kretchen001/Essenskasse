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

        /// <summary>Empty ctor.</summary>
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
            switch (method) {
                case CalculationOperation.Addition: {
                        Money saldo = new Money(0, 0);
                        long temp1 = money1.Cent + (100 * money1.Euro);
                        long temp2 = money2.Cent + (100 * money2.Euro);
                        long tempR = temp1 + temp2;
                        saldo.Cent = (short)(tempR % 100);
                        saldo.Euro = (long)((tempR - saldo.Cent) * 0.01); // multiply is faster then divide
                        return saldo;
                    }
                case CalculationOperation.Substraction: {
                        Money saldo = new Money(0, 0);
                        long temp1 = money1.Cent + (100 * money1.Euro);
                        long temp2 = money2.Cent + (100 * money2.Euro);
                        long tempR = temp1 - temp2;
                        saldo.Cent = (short)(tempR % 100);
                        saldo.Euro = (long)((tempR - saldo.Cent) * 0.01); // multiply is faster then divide
                        return saldo;
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
