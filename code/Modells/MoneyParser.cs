namespace Modells {

    /// <summary>Wandelt eine eingetippte Betragsangabe (z. B. "12,5", "12.50", "1.234,56", "-3") in einen Geldbetrag um.</summary>
    public static class MoneyParser {

        private const int MaxEuroDigits = 9;

        /// <summary>
        /// Liest einen Betrag. Erlaubt sind Ziffern, ein Komma oder Punkt als Dezimaltrennzeichen mit ein oder zwei Nachkommastellen,
        /// ein fuehrendes Minus, Leerzeichen und ein Waehrungszeichen (oder -code). Kommen Komma und Punkt vor, gilt das letzte als Dezimaltrennzeichen
        /// und das andere als Tausendertrennzeichen. Mehr als zwei Nachkommastellen und alles andere sind ungueltig
        /// (es wird nie stillschweigend gerundet oder abgeschnitten). Der Wert 0 ist gueltig, die Pruefung auf 0 ist Sache des Aufrufers.
        /// </summary>
        public static bool TryParse(string? input, out Money money) {
            money = new Money(0, 0);
            if (input is null) {
                return false;
            }
            string text = Currency.StripSymbols(input).Replace(" ", "").Replace("\u00A0", "").Trim();
            if (text.Length == 0) {
                return false;
            }

            bool negative = false;
            if (text[0] == '-') {
                negative = true;
                text = text[1..];
            }
            if (text.Length == 0 || text.Any(c => !(char.IsAsciiDigit(c) || c == ',' || c == '.'))) {
                return false;
            }

            string euroPart = text;
            string centPart = "";
            int lastSeparator = text.LastIndexOfAny(new[] { ',', '.' });
            if (lastSeparator >= 0) {
                char decimalSeparator = text[lastSeparator];
                char thousandsSeparator = decimalSeparator == ',' ? '.' : ',';
                // Das Dezimaltrennzeichen darf nur einmal vorkommen; das andere gilt als Tausendertrennzeichen.
                string before = text[..lastSeparator];
                if (before.Contains(decimalSeparator)) {
                    return false;
                }
                euroPart = before.Replace(thousandsSeparator.ToString(), "");
                centPart = text[(lastSeparator + 1)..];
                if (centPart.Length < 1 || centPart.Length > 2) {
                    return false;
                }
            }

            if (euroPart.Length > MaxEuroDigits) {
                return false;
            }
            if (euroPart.Length == 0 && centPart.Length == 0) {
                return false;
            }

            long euro = euroPart.Length == 0 ? 0 : long.Parse(euroPart);
            long cent = centPart.Length == 0 ? 0 : long.Parse(centPart.PadRight(2, '0'));
            long totalCents = euro * 100 + cent;
            money = Money.FromTotalCents(negative ? -totalCents : totalCents);
            return true;
        }
    }
}
