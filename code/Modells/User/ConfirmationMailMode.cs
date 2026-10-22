namespace Modells.User {

    /// <summary>Legt fest, ob bei einer Einzahlung eine Bestaetigungsmail vorbereitet wird.</summary>
    public enum ConfirmationMailMode {

        /// <summary>Es wird nie eine Bestaetigungsmail vorbereitet.</summary>
        Never = 0,

        /// <summary>Bei jeder Einzahlung kann gewaehlt werden, ob eine Bestaetigungsmail vorbereitet wird.</summary>
        Optional = 1,

        /// <summary>Es wird immer eine Bestaetigungsmail vorbereitet (verpflichtend).</summary>
        Always = 2
    }

    /// <summary>Hilfsfunktionen zum Einlesen der Einstellung aus aelteren und neuen Speicherformaten.</summary>
    public static class ConfirmationMailModes {

        /// <summary>Liest den Modus aus 0/1/2 sowie aus den frueheren Werten "True"/"False".
        /// Unbekannte oder leere Werte ergeben <paramref name="fallback"/>.</summary>
        public static ConfirmationMailMode Parse(string? text, ConfirmationMailMode fallback = ConfirmationMailMode.Optional) {
            string value = (text ?? "").Trim();
            if (value.Equals("True", StringComparison.OrdinalIgnoreCase)) {
                return ConfirmationMailMode.Optional;
            }
            if (value.Equals("False", StringComparison.OrdinalIgnoreCase)) {
                return ConfirmationMailMode.Never;
            }
            if (int.TryParse(value, out int number) && Enum.IsDefined(typeof(ConfirmationMailMode), number)) {
                return (ConfirmationMailMode)number;
            }
            return fallback;
        }
    }
}
