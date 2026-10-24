namespace Modells {

    /// <summary>Hilfen fuer die Aenderungen-Seite: Was wurde gegenueber der Buchung veraendert, und wie steht ein Betrag im Eingabefeld.</summary>
    public static class EntityChanges {

        /// <summary>Der Betrag als Eingabetext mit Komma, z. B. "12,50" oder "-3,05".</summary>
        public static string ToInputText(Money money) {
            long cents = money.ToTotalCents();
            long abs = Math.Abs(cents);
            return (cents < 0 ? "-" : "") + (abs / 100) + "," + (abs % 100).ToString("00");
        }

        /// <summary>Unterscheidet sich Betrag, Kommentar oder Person von der Einzahlung? Leerzeichen am Rand des Kommentars zaehlen nicht.</summary>
        public static bool PaymentChanged(Payment original, Money amount, string? comment, Guid person) {
            return amount.ToTotalCents() != original.Amount.ToTotalCents()
                || !string.Equals((comment ?? "").Trim(), (original.Comment ?? "").Trim(), StringComparison.Ordinal)
                || person != original.ReferencedPerson;
        }

        /// <summary>Unterscheidet sich Betrag, Anbieter oder Person von der Bestellung?</summary>
        public static bool OrderChanged(Order original, Money amount, Guid provider, Guid person) {
            return amount.ToTotalCents() != original.Money.ToTotalCents()
                || provider != original.Provider.Guid
                || person != original.ReferencedPerson;
        }
    }
}
