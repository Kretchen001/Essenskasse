using System.Globalization;

namespace Modells {

    /// <summary>Baut Betreff und Text der Bestaetigungsmail fuer eine Einzahlung aus uebersetzten Textbausteinen.</summary>
    public static class PaymentConfirmation {

        /// <summary>
        /// bodyTemplate hat die Platzhalter {0} Name, {1} Datum, {2} Uhrzeit, {3} Betrag; commentTemplate den Platzhalter {0} Kommentar.
        /// Ohne Kommentar entfaellt die Zeile mit der Anmerkung.
        /// </summary>
        public static (string Subject, string Body) Compose(string subject, string bodyTemplate, string commentTemplate, string closing,
                                                            string personName, DateTime when, Money amount, string? comment,
                                                            IFormatProvider? culture = null) {
            IFormatProvider format = culture ?? CultureInfo.CurrentCulture;
            string body = string.Format(format, bodyTemplate,
                personName, when.ToString("d", format), when.ToString("t", format), amount.ToString());
            if (!string.IsNullOrWhiteSpace(comment)) {
                body += "\n" + string.Format(format, commentTemplate, comment.Trim());
            }
            body += "\n\n" + closing;
            return (subject, body);
        }
    }
}
