namespace Modells.Statistics {

    /// <summary>Bestellungen eines Anbieters im Zeitraum: Summe und Anzahl (Stornobuchungen sind abgezogen).</summary>
    /// <param name="AmountShare">Anteil an der Gesamtsumme von 0 bis 1.</param>
    public sealed record ProviderShare(Guid Provider, string Name, long Cents, int Count, double AmountShare) {

        /// <summary>Durchschnittlicher Betrag je Bestellung in Cent, 0 ohne Bestellungen.</summary>
        public long AverageCents => Count <= 0 ? 0 : Cents / Count;
    }

    /// <summary>Anteile der Anbieter an den Bestellungen.</summary>
    public static class ProviderShares {

        /// <summary>Gruppiert die Bestellungen der Tage <paramref name="from"/> bis <paramref name="to"/> nach Anbieter, groesste Summe zuerst.
        /// Eine Stornobuchung (negativer Betrag) zaehlt als minus eine Bestellung.</summary>
        public static List<ProviderShare> Build(IReadOnlyList<Order> orders, DateTime from, DateTime to) {
            var groups = orders.Where(x => x.DateTime.Date >= from.Date && x.DateTime.Date <= to.Date)
                .GroupBy(x => x.Provider.Guid)
                .Select(g => (Guid: g.Key,
                              Name: g.OrderByDescending(x => x.DateTime).First().Provider.Designation ?? "",
                              Cents: g.Sum(x => x.Money.ToTotalCents()),
                              Count: g.Sum(x => x.Money.ToTotalCents() < 0 ? -1 : 1)))
                .Where(x => x.Count != 0 || x.Cents != 0)
                .ToList();
            long total = groups.Sum(x => x.Cents);
            return groups.OrderByDescending(x => x.Cents).ThenBy(x => x.Name)
                .Select(x => new ProviderShare(x.Guid, x.Name, x.Cents, x.Count, total <= 0 ? 0 : (double)x.Cents / total))
                .ToList();
        }
    }
}
