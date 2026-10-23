using System.Globalization;
using System.Text.RegularExpressions;

namespace Modells {

    /// <summary>Datumsausgaben, die sich nach der Sprache (Kultur) der App richten statt fest deutsch zu sein.
    /// Ohne Angabe wird <see cref="CultureInfo.CurrentCulture"/> benutzt.</summary>
    public static class DateText {

        /// <summary>Kurzes Datum ohne Jahr in der Reihenfolge der Kultur, z. B. 23.10 (de), 10/23 (en-US), 23/10 (es).</summary>
        public static string DayMonth(DateTime date, CultureInfo? culture = null) {
            culture ??= CultureInfo.CurrentCulture;
            string pattern = culture.DateTimeFormat.ShortDatePattern;
            string withoutYear = Regex.IsMatch(pattern, "y+$")
                ? Regex.Replace(pattern, "[^dMy]*y+$", "")
                : Regex.Replace(pattern, "y+[^dMy]*", "");
            return date.ToString(withoutYear, culture);
        }

        /// <summary>Kurzer Wochentag und Tag ohne Jahr, z. B. "Fr, 23.10".</summary>
        public static string WeekdayDayMonth(DateTime date, CultureInfo? culture = null) {
            culture ??= CultureInfo.CurrentCulture;
            return $"{date.ToString("ddd", culture)}, {DayMonth(date, culture)}";
        }

        /// <summary>Kurzer Wochentag, Tag ohne Jahr und Uhrzeit, z. B. "Fr, 23.10 17:24".</summary>
        public static string WeekdayDayMonthTime(DateTime date, CultureInfo? culture = null) {
            culture ??= CultureInfo.CurrentCulture;
            return $"{WeekdayDayMonth(date, culture)} {date.ToString("t", culture)}";
        }

        /// <summary>Kurzer Wochentag und vollstaendiges kurzes Datum, z. B. "Fr, 23.10.2026".</summary>
        public static string WeekdayDate(DateTime date, CultureInfo? culture = null) {
            culture ??= CultureInfo.CurrentCulture;
            return $"{date.ToString("ddd", culture)}, {date.ToString("d", culture)}";
        }

        /// <summary>Kurzer Wochentag und Tageszahl, z. B. "Fr 23" (fuer die Tagesknoepfe).</summary>
        public static string WeekdayDay(DateTime date, CultureInfo? culture = null) {
            culture ??= CultureInfo.CurrentCulture;
            return $"{date.ToString("ddd", culture)} {date.Day}";
        }
    }
}
