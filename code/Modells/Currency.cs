namespace Modells {

    /// <summary>Eine waehlbare Waehrung. Betraege werden immer als ganze Einheiten plus zwei Nachkommastellen gespeichert;
    /// die Waehrung bestimmt nur die Darstellung (Zeichen, Stellung, Dezimaltrennzeichen) und rechnet nichts um.</summary>
    /// <param name="Code">ISO-4217-Code, z. B. "EUR".</param>
    /// <param name="Symbol">Anzeigezeichen, z. B. "€".</param>
    /// <param name="SymbolFirst">true: Zeichen vor dem Betrag ("$12.50"), sonst dahinter ("12,50 €").</param>
    /// <param name="DecimalSeparator">Dezimaltrennzeichen in der Anzeige.</param>
    public sealed record Currency(string Code, string Symbol, bool SymbolFirst, char DecimalSeparator) {

        /// <summary>Euro, die Standardwaehrung.</summary>
        public static readonly Currency Euro = new("EUR", "€", false, ',');

        /// <summary>Alle waehlbaren Waehrungen (nur solche mit zwei Nachkommastellen).</summary>
        public static IReadOnlyList<Currency> All { get; } = [
            Euro,
            new("USD", "$", true, '.'),
            new("GBP", "£", true, '.'),
            new("CHF", "CHF", false, '.'),
            new("CAD", "$", true, '.'),
            new("AUD", "$", true, '.'),
            new("PLN", "zł", false, ','),
            new("CZK", "Kč", false, ','),
            new("SEK", "kr", false, ','),
            new("NOK", "kr", false, ','),
            new("DKK", "kr", false, ','),
            new("RUB", "₽", false, ','),
            new("TRY", "₺", true, ',')
        ];

        /// <summary>Die Waehrung zum Code; unbekannt oder leer ergibt Euro.</summary>
        public static Currency FromCode(string? code) {
            return All.FirstOrDefault(x => x.Code == code) ?? Euro;
        }

        /// <summary>Ist der Code eine waehlbare Waehrung?</summary>
        public static bool IsKnown(string? code) {
            return All.Any(x => x.Code == code);
        }

        /// <summary>Setzt das Zeichen an den bereits formatierten Betrag, z. B. ("12,50") zu "12,50 €" bzw. ("12.50", negativ) in Dollar zu "-$12.50".</summary>
        public string Format(string number, bool negative = false) {
            string sign = negative ? "-" : "";
            return SymbolFirst ? sign + Symbol + number : sign + number + " " + Symbol;
        }

        /// <summary>Entfernt alle bekannten Waehrungszeichen und -codes aus einem Text (fuer das Einlesen von Betraegen).</summary>
        public static string StripSymbols(string text) {
            foreach (Currency currency in All) {
                text = text.Replace(currency.Code, "", StringComparison.OrdinalIgnoreCase);
            }
            foreach (Currency currency in All) {
                text = text.Replace(currency.Symbol, "", StringComparison.Ordinal);
            }
            return text;
        }
    }
}
