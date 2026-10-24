namespace Modells.Statistics {

    /// <summary>Ein Wert (Cent) an einem Tag.</summary>
    public sealed record DayValue(DateTime Day, long Cents);

    /// <summary>Verlauf des Kassenstands (Summe aller Salden) ueber die Zeit.</summary>
    public static class CashHistory {

        /// <summary>Der Kassenstand am Ende jedes Tages von <paramref name="from"/> bis <paramref name="to"/> (beide eingeschlossen).
        /// Der Anfangsstand ergibt sich aus allen Buchungen vor <paramref name="from"/>; Tage ohne Buchung behalten den Stand.</summary>
        public static List<DayValue> Daily(IReadOnlyList<BalanceEvent> events, DateTime from, DateTime to) {
            List<DayValue> days = [];
            DateTime start = from.Date;
            DateTime end = to.Date;
            if (end < start) {
                return days;
            }
            long running = events.Where(x => x.When.Date < start).Sum(x => x.Cents);
            Dictionary<DateTime, long> perDay = events
                .Where(x => x.When.Date >= start && x.When.Date <= end)
                .GroupBy(x => x.When.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Cents));
            for (DateTime day = start; day <= end; day = day.AddDays(1)) {
                running += perDay.GetValueOrDefault(day);
                days.Add(new DayValue(day, running));
            }
            return days;
        }

        /// <summary>Verringert die Zahl der Punkte auf hoechstens etwa <paramref name="maxPoints"/> (jeder n-te Punkt; der letzte bleibt immer).</summary>
        public static List<DayValue> Downsample(IReadOnlyList<DayValue> points, int maxPoints) {
            if (maxPoints < 2 || points.Count <= maxPoints) {
                return points.ToList();
            }
            int step = (int)Math.Ceiling(points.Count / (double)maxPoints);
            List<DayValue> result = [];
            for (int i = 0; i < points.Count; i += step) {
                result.Add(points[i]);
            }
            if (result[^1] != points[^1]) {
                result.Add(points[^1]);
            }
            return result;
        }
    }
}
