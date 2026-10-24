namespace Modells {

    /// <summary>Die Tage einer Woche (Montag bis Sonntag), z. B. fuer die Tagesauswahl bei Bestellungen.</summary>
    public static class WeekDays {

        /// <summary>Alle sieben Tage der Woche, in der <paramref name="day"/> liegt, von Montag bis Sonntag (jeweils 00:00 Uhr).</summary>
        public static IReadOnlyList<DateTime> OfWeek(DateTime day) {
            int sinceMonday = ((int)day.DayOfWeek + 6) % 7;
            DateTime monday = day.Date.AddDays(-sinceMonday);
            return Enumerable.Range(0, 7).Select(offset => monday.AddDays(offset)).ToList();
        }
    }
}
