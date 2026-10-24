namespace Modells.User {

    /// <summary>Prueft und bereinigt Einstellungen, bevor der Server sie speichert: Werte werden in sinnvolle Grenzen gebracht,
    /// Texte gegen eine Liste erlaubter Werte geprueft und der Aenderungstoken wird nie uebernommen
    /// (er gehoert zum Benutzer, nicht zu den Einstellungen).</summary>
    public static class SettingsSanitizer {

        /// <summary>Kleinste und groesste Anzahl nachzuladender Eintraege.</summary>
        public const int MinLoadAmount = 1;
        /// <summary></summary>
        public const int MaxLoadAmount = 500;
        /// <summary>Laengste erlaubte Admin-Adresse (Spaltenlaenge in der Datenbank).</summary>
        public const int MaxEmailLength = 255;
        /// <summary>Laengster Name eines Sortierwerts.</summary>
        public const int MaxSortLength = 32;

        /// <summary>Erlaubte Farbschemata.</summary>
        public static IReadOnlyList<string> Themes { get; } = ["Light", "Dark"];

        /// <summary>Erlaubte Sprachen.</summary>
        public static IReadOnlyList<string> Languages { get; } = ["de-DE", "en-US", "es-ES", "ru"];

        /// <summary>Erlaubte Waehrungen (Codes).</summary>
        public static IReadOnlyList<string> Currencies { get; } = Modells.Currency.All.Select(x => x.Code).ToList();

        /// <summary>Gibt eine bereinigte Kopie zurueck. Unbekannte oder ungueltige Texte werden zu "" (= nicht gespeichert).</summary>
        public static Settings Sanitize(Settings settings) {
            string email = (settings.AdminEmail ?? "").Trim();
            return new Settings {
                IsSingleListWishDashboard = settings.IsSingleListWishDashboard,
                LoadAmountPayment = Math.Clamp(settings.LoadAmountPayment, MinLoadAmount, MaxLoadAmount),
                LoadAmountOrder = Math.Clamp(settings.LoadAmountOrder, MinLoadAmount, MaxLoadAmount),
                FilterShowProvider = settings.FilterShowProvider,
                DashboardMoneyLimit = settings.DashboardMoneyLimit ?? new Money(0, 0),
                FilterShowPerson = settings.FilterShowPerson,
                AccessToken = null,
                AdminEmail = email.Length > MaxEmailLength ? email[..MaxEmailLength] : email,
                NoticeableOnRightSide = settings.NoticeableOnRightSide,
                SendConfirmationMail = Enum.IsDefined(settings.SendConfirmationMail) ? settings.SendConfirmationMail : ConfirmationMailMode.Optional,
                QuickAmounts = settings.QuickAmounts is { Count: > 0 } ? Modells.QuickAmounts.Normalize(settings.QuickAmounts) : [],
                Theme = Themes.Contains(settings.Theme) ? settings.Theme : "",
                Language = Languages.Contains(settings.Language) ? settings.Language : "",
                PersonSort = SafeSortName(settings.PersonSort),
                ProviderSort = SafeSortName(settings.ProviderSort),
                Currency = Currencies.Contains(settings.Currency) ? settings.Currency : ""
            };
        }

        /// <summary>Ein unterstuetzter Sprachcode, sonst "" (= Standard Deutsch).</summary>
        public static string SafeLanguage(string? language) {
            return language is not null && Languages.Contains(language) ? language : "";
        }

        /// <summary>Ein Sortierwert besteht nur aus Buchstaben und ist hoechstens <see cref="MaxSortLength"/> lang; sonst "".</summary>
        public static string SafeSortName(string? name) {
            if (string.IsNullOrEmpty(name) || name.Length > MaxSortLength || !name.All(char.IsAsciiLetter)) {
                return "";
            }
            return name;
        }

        /// <summary>Schnellbetraege als Text fuer die Datenbank, z. B. "100,250,500".</summary>
        public static string QuickAmountsToText(IEnumerable<long>? cents) {
            return string.Join(",", Modells.QuickAmounts.Normalize(cents));
        }

        /// <summary>Liest Schnellbetraege aus dem Datenbanktext. Leer oder unlesbar ergibt eine leere Liste (= nicht gespeichert).</summary>
        public static List<long> QuickAmountsFromText(string? text) {
            if (string.IsNullOrWhiteSpace(text)) {
                return [];
            }
            List<long> values = [];
            foreach (string part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
                if (long.TryParse(part, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out long value)) {
                    values.Add(value);
                }
            }
            return values.Count == 0 ? [] : Modells.QuickAmounts.Normalize(values);
        }
    }
}
