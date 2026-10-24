namespace Modells.Statistics {

    /// <summary>Einzahlungen und Bestellungen eines Monats (Cent, beide als positive Summen; Stornobuchungen sind abgezogen).</summary>
    public sealed record MonthFlow(DateTime Month, long PaymentCents, long OrderCents) {

        /// <summary>Einzahlungen minus Bestellungen.</summary>
        public long NetCents => PaymentCents - OrderCents;
    }

    /// <summary>Einzahlungen und Bestellungen je Monat.</summary>
    public static class MonthlyFlow {

        /// <summary>Ein Eintrag je Monat von dem Monat von <paramref name="from"/> bis zu dem Monat von <paramref name="to"/>
        /// (auch Monate ohne Buchungen). Gezaehlt werden Buchungen vom Tag <paramref name="from"/> bis zum Tag <paramref name="to"/>.</summary>
        public static List<MonthFlow> Build(IReadOnlyList<Payment> payments, IReadOnlyList<Order> orders, DateTime from, DateTime to) {
            List<MonthFlow> months = [];
            if (to.Date < from.Date) {
                return months;
            }
            static DateTime MonthOf(DateTime date) => new(date.Year, date.Month, 1);
            bool InRange(DateTime when) => when.Date >= from.Date && when.Date <= to.Date;
            Dictionary<DateTime, long> paid = payments.Where(x => InRange(x.DateTime))
                .GroupBy(x => MonthOf(x.DateTime)).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount.ToTotalCents()));
            Dictionary<DateTime, long> ordered = orders.Where(x => InRange(x.DateTime))
                .GroupBy(x => MonthOf(x.DateTime)).ToDictionary(g => g.Key, g => g.Sum(x => x.Money.ToTotalCents()));
            for (DateTime month = MonthOf(from); month <= MonthOf(to); month = month.AddMonths(1)) {
                months.Add(new MonthFlow(month, paid.GetValueOrDefault(month), ordered.GetValueOrDefault(month)));
            }
            return months;
        }
    }
}
