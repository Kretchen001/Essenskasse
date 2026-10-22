namespace Modells {

    /// <summary>Ein Tag der Bestelluebersicht: Summe je Person und Tagessumme, alles in ganzen Cent.</summary>
    public sealed record OrderOverviewDay(DateTime Date, IReadOnlyDictionary<Guid, long> CentsByPerson, long TotalCents);

    /// <summary>Fasst Bestellungen zu Tagen zusammen (neuester Tag zuerst). Mehrere Bestellungen derselben Person am selben Tag
    /// werden addiert; aus mehreren Ladeschritten darf dieselbe Liste immer wieder neu aufgebaut werden.</summary>
    public static class OrderOverview {

        public static List<OrderOverviewDay> Build(IEnumerable<Order> orders) {
            return orders
                .GroupBy(x => x.DateTime.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => {
                    Dictionary<Guid, long> perPerson = g
                        .GroupBy(o => o.ReferencedPerson)
                        .ToDictionary(p => p.Key, p => p.Sum(o => o.Money.ToTotalCents()));
                    return new OrderOverviewDay(g.Key, perPerson, perPerson.Values.Sum());
                })
                .ToList();
        }

        /// <summary>Die Personen, die in den Tagen mindestens eine Bestellung haben (fuer die Spalten der Tabelle).</summary>
        public static HashSet<Guid> PeopleWithOrders(IEnumerable<OrderOverviewDay> days) {
            return days.SelectMany(d => d.CentsByPerson.Keys).ToHashSet();
        }
    }
}
