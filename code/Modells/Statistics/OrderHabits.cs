namespace Modells.Statistics {

    /// <summary>Wie wahrscheinlich eine Person an einem Wochentag bestellt, aus dem bisherigen Bestell-Log geschaetzt.</summary>
    /// <param name="OrderDays">Anzahl Tage, an denen die Person (netto) bestellt hat.</param>
    /// <param name="Orders">Anzahl Bestellungen (netto, Stornos abgezogen).</param>
    /// <param name="WeekdayProbability">Je Wochentag (0 = Montag bis 6 = Sonntag): Anteil der Tage dieses Wochentags im Zeitraum, an denen bestellt wurde (0 bis 1).</param>
    /// <param name="WeekdayDays">Je Wochentag: wie viele solche Tage der Zeitraum enthaelt (Nenner).</param>
    /// <param name="TopProviders">Anbieter der Person mit Anteil an ihren Bestellungen, haeufigster zuerst.</param>
    public sealed record PersonHabit(Person Person, int OrderDays, int Orders, IReadOnlyList<double> WeekdayProbability,
        IReadOnlyList<int> WeekdayDays, IReadOnlyList<(string Provider, double Share)> TopProviders) {

        /// <summary>Wenige Bestelltage: die Prozentwerte sind dann nur grobe Schaetzungen.</summary>
        public bool SmallSample => OrderDays < OrderHabits.SmallSampleDays;
    }

    /// <summary>Empirische Bestell-Wahrscheinlichkeiten (relative Haeufigkeiten) je Person.</summary>
    public static class OrderHabits {

        /// <summary>Unter dieser Zahl Bestelltage gilt die Stichprobe als klein.</summary>
        public const int SmallSampleDays = 10;

        /// <summary>Montag = 0 bis Sonntag = 6.</summary>
        public static int WeekdayIndex(DateTime date) => ((int)date.DayOfWeek + 6) % 7;

        /// <summary>Beobachtet werden die Tage <paramref name="from"/> bis <paramref name="to"/>, hoechstens bis <paramref name="asOf"/> (Tag).
        /// Personen ohne Bestellung im Zeitraum fehlen in der Liste; sortiert nach Bestellungen, meiste zuerst.</summary>
        public static List<PersonHabit> Build(StatisticsData data, DateTime from, DateTime to, DateTime asOf, int topProviders = 3) {
            DateTime start = from.Date;
            DateTime end = to.Date < asOf.Date ? to.Date : asOf.Date;
            List<PersonHabit> result = [];
            if (end < start) {
                return result;
            }
            int[] weekdayDays = new int[7];
            for (DateTime day = start; day <= end; day = day.AddDays(1)) {
                weekdayDays[WeekdayIndex(day)]++;
            }
            foreach (IGrouping<Guid, Order> byPerson in data.Orders.Where(x => x.DateTime.Date >= start && x.DateTime.Date <= end).GroupBy(x => x.ReferencedPerson)) {
                Person? person = data.Persons.FirstOrDefault(x => x.Guid == byPerson.Key);
                if (person is null) {
                    continue;
                }
                int Sign(Order x) => x.Money.ToTotalCents() < 0 ? -1 : 1;
                List<DateTime> orderDays = byPerson.GroupBy(x => x.DateTime.Date).Where(g => g.Sum(Sign) > 0).Select(g => g.Key).ToList();
                int orders = byPerson.Sum(Sign);
                if (orders <= 0) {
                    continue;
                }
                int[] perWeekday = new int[7];
                foreach (DateTime day in orderDays) {
                    perWeekday[WeekdayIndex(day)]++;
                }
                double[] probability = Enumerable.Range(0, 7).Select(i => weekdayDays[i] == 0 ? 0 : (double)perWeekday[i] / weekdayDays[i]).ToArray();
                List<(string, double)> providers = byPerson.GroupBy(x => x.Provider.Guid)
                    .Select(g => (Name: g.OrderByDescending(x => x.DateTime).First().Provider.Designation ?? "", Count: g.Sum(Sign)))
                    .Where(x => x.Count > 0)
                    .OrderByDescending(x => x.Count).ThenBy(x => x.Name)
                    .Take(topProviders)
                    .Select(x => (x.Name, (double)x.Count / orders))
                    .ToList();
                result.Add(new PersonHabit(person, orderDays.Count, orders, probability, weekdayDays, providers));
            }
            return result.OrderByDescending(x => x.Orders).ThenBy(x => x.Person.Name).ToList();
        }
    }
}
