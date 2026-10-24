using System.Globalization;
using System.Text;

namespace Modells {

    /// <summary>Eine Buchung fuer den Export. <paramref name="Amount"/> ist die Wirkung auf den Saldo
    /// (Einzahlungen positiv, Bestellungen negativ).</summary>
    public sealed record BookingRow(DateTime When, string Kind, string Person, string Detail, Money Amount);

    /// <summary>Baut eine CSV-Datei aus Buchungen: Semikolon als Trennzeichen (Excel im deutschen Raum), Zeilenende CRLF,
    /// Datum als JJJJ-MM-TT, Betrag mit dem Dezimaltrennzeichen der Waehrung und ohne Waehrungszeichen.</summary>
    public static class BookingCsv {

        /// <summary>Trennzeichen der Spalten.</summary>
        public const char Delimiter = ';';

        /// <summary>Kopfzeile (<paramref name="headers"/>) und je Buchung eine Zeile:
        /// Datum, Uhrzeit, Art, Person, Anmerkung/Anbieter, Betrag, Waehrung.</summary>
        public static string Build(IEnumerable<BookingRow> rows, IReadOnlyList<string> headers, Currency currency) {
            StringBuilder csv = new();
            csv.Append(string.Join(Delimiter, headers.Select(Field))).Append("\r\n");
            foreach (BookingRow row in rows) {
                string[] cells = [
                    row.When.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    row.When.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                    Field(row.Kind),
                    Field(row.Person),
                    Field(row.Detail),
                    Amount(row.Amount, currency),
                    currency.Code
                ];
                csv.Append(string.Join(Delimiter, cells)).Append("\r\n");
            }
            return csv.ToString();
        }

        /// <summary>Betrag als Zahl, z. B. "-12,50" (ohne Waehrungszeichen).</summary>
        public static string Amount(Money amount, Currency currency) {
            long cents = amount.ToTotalCents();
            long abs = Math.Abs(cents);
            return (cents < 0 ? "-" : "") + (abs / 100).ToString(CultureInfo.InvariantCulture)
                + currency.DecimalSeparator + (abs % 100).ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>Ein Textfeld: Anfuehrungszeichen verdoppelt und das Feld bei Trennzeichen oder Zeilenumbruch eingerahmt.
        /// Beginnt der Text mit =, +, - oder @, wird ein Apostroph vorangestellt, damit Excel ihn nicht als Formel ausfuehrt.</summary>
        public static string Field(string? value) {
            string text = value ?? "";
            if (text.Length > 0 && "=+-@\t\r".Contains(text[0])) {
                text = "'" + text;
            }
            if (text.IndexOfAny([Delimiter, '"', '\r', '\n']) >= 0) {
                text = "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            return text;
        }
    }
}
