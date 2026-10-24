namespace Modells.Statistics {

    /// <summary>Eine Veraenderung des Saldos einer Person: Einzahlungen positiv, Bestellungen negativ.
    /// Stornobuchungen sind gewoehnliche Buchungen mit umgekehrtem Betrag und heben ihr Gegenstueck dadurch auf.</summary>
    public sealed record BalanceEvent(DateTime When, Guid Person, long Cents);

    /// <summary>Die Daten, aus denen die Auswertungen rechnen.</summary>
    public sealed class StatisticsData {

        /// <summary>Alle (sichtbaren) Personen.</summary>
        public IReadOnlyList<Person> Persons { get; }
        /// <summary>Alle Einzahlungen.</summary>
        public IReadOnlyList<Payment> Payments { get; }
        /// <summary>Alle Bestellungen.</summary>
        public IReadOnlyList<Order> Orders { get; }
        /// <summary>Alle Saldo-Veraenderungen, nach Zeit sortiert.</summary>
        public IReadOnlyList<BalanceEvent> Events { get; }

        public StatisticsData(IReadOnlyList<Person> persons, IReadOnlyList<Payment> payments, IReadOnlyList<Order> orders) {
            Persons = persons;
            Payments = payments;
            Orders = orders;
            Events = payments.Select(x => new BalanceEvent(x.DateTime, x.ReferencedPerson, x.Amount.ToTotalCents()))
                .Concat(orders.Select(x => new BalanceEvent(x.DateTime, x.ReferencedPerson, -x.Money.ToTotalCents())))
                .OrderBy(x => x.When)
                .ToList();
        }

        /// <summary>Der erste und der letzte Tag mit Buchungen; ohne Buchungen null.</summary>
        public (DateTime First, DateTime Last)? Span =>
            Events.Count == 0 ? null : (Events[0].When.Date, Events[^1].When.Date);
    }
}
