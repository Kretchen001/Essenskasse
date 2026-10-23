namespace Modells {

    /// <summary>Die einstellbaren Schnellbetraege (Chips) im Bestellfenster: mindestens einer, hoechstens <see cref="MaxCount"/>,
    /// aufsteigend sortiert, ohne Doppelte. Alle Betraege sind ganze Cent.</summary>
    public static class QuickAmounts {

        /// <summary>Hoechstzahl an Chips.</summary>
        public const int MaxCount = 12;

        /// <summary>Hoechster erlaubter Einzelbetrag in Cent (100.000 Euro).</summary>
        public const long MaxCents = 10_000_000;

        /// <summary>Voreinstellung: 1,00 / 2,50 / 5,00 / 10,00 Euro.</summary>
        public static IReadOnlyList<long> Default { get; } = new long[] { 100, 250, 500, 1000 };

        /// <summary>Bereinigt eine Liste: entfernt ungueltige und doppelte Betraege, sortiert, begrenzt die Anzahl.
        /// Bleibt nichts uebrig, gilt die Voreinstellung (es gibt immer mindestens einen Chip).</summary>
        public static List<long> Normalize(IEnumerable<long>? cents) {
            List<long> result = (cents ?? [])
                .Where(x => x > 0 && x <= MaxCents)
                .Distinct()
                .OrderBy(x => x)
                .Take(MaxCount)
                .ToList();
            return result.Count == 0 ? [.. Default] : result;
        }

        /// <summary>Fuegt einen eingetippten Betrag hinzu (z. B. "2,5"). Ungueltig sind 0, negative Betraege, zu hohe
        /// Betraege und Eingaben, die kein Betrag sind; ebenso, wenn die Hoechstzahl erreicht ist. Ein bereits vorhandener
        /// Betrag gilt als erfolgreich und wird nicht doppelt aufgenommen.</summary>
        public static bool TryAdd(IEnumerable<long> current, string? text, out List<long> result) {
            result = Normalize(current);
            if (!MoneyParser.TryParse(text, out Money parsed)) {
                return false;
            }
            long cents = parsed.ToTotalCents();
            if (cents <= 0 || cents > MaxCents) {
                return false;
            }
            if (result.Contains(cents)) {
                return true;
            }
            if (result.Count >= MaxCount) {
                return false;
            }
            result.Add(cents);
            result.Sort();
            return true;
        }

        /// <summary>Entfernt einen Betrag; der letzte verbleibende Chip kann nicht entfernt werden.</summary>
        public static List<long> Remove(IEnumerable<long> current, long cents) {
            List<long> result = Normalize(current);
            if (result.Count > 1) {
                result.Remove(cents);
            }
            return result;
        }

        /// <summary>Betrag als Text fuer ein Eingabefeld, z. B. "2,50" (ohne Waehrungszeichen, immer zwei Nachkommastellen).</summary>
        public static string ToInputText(long cents) {
            long abs = Math.Abs(cents);
            return (cents < 0 ? "-" : "") + (abs / 100) + "," + (abs % 100).ToString("00");
        }

        /// <summary>Kurze Beschriftung in der aktuellen Waehrung: "5 €" bei ganzen Einheiten, sonst "2,50 €".</summary>
        public static string Label(long cents) {
            if (cents % 100 == 0) {
                return Money.CurrentCurrency.Format((cents / 100).ToString());
            }
            return Money.FromTotalCents(cents).ToString();
        }
    }
}
