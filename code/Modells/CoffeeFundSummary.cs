namespace Modells {

    /// <summary>Kassenstand und Summen der Kaffeekasse, ausschliesslich in ganzen Cent gerechnet.</summary>
    public static class CoffeeFundSummary {

        /// <summary>Ergebnis der Auswertung: Eingaenge, Ausgaenge (als positive Betraege) und der Kassenstand.</summary>
        public readonly record struct Result(Money Income, Money Expense, Money Balance);

        /// <summary>Summiert alle Buchungen. Positive Betraege sind Einzahlungen, negative Auszahlungen.</summary>
        public static Result Summarize(IEnumerable<TransactionProcedure> transactions) {
            long income = 0;
            long expense = 0;
            foreach (TransactionProcedure x in transactions) {
                long cents = x.Amount.ToTotalCents();
                if (cents >= 0) {
                    income = checked(income + cents);
                }
                else {
                    expense = checked(expense - cents);
                }
            }
            return new Result(Money.FromTotalCents(income), Money.FromTotalCents(expense), Money.FromTotalCents(income - expense));
        }
    }
}
