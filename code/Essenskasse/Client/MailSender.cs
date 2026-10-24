using Essenskasse.Client.Localization;
using Microsoft.JSInterop;
using Modells;
using System.Globalization;

namespace Essenskasse.Client {

    /// <summary>Oeffnet das Mailprogramm mit einer Mail in zwei Sprachen: zuerst die Sprache des Empfaengers,
    /// danach die Sprache des angemeldeten Benutzers (die der Oberflaeche). Sind beide gleich, kommt der Text nur einmal.</summary>
    public static class MailSender {

        /// <summary>Ein unterstuetzter Sprachcode; leer oder unbekannt ergibt Deutsch.</summary>
        public static string Normalize(string? language) {
            CultureWithName? found = LocalizerSettings.SupportedCultures.FirstOrDefault(x => string.Equals(x.Culture, language, StringComparison.OrdinalIgnoreCase));
            return found?.Culture ?? LocalizerSettings.NeutralCulture.Culture;
        }

        /// <summary>Die Sprache der Oberflaeche (des angemeldeten Benutzers) als unterstuetzter Sprachcode.</summary>
        public static string UserLanguage() {
            CultureInfo current = CultureInfo.CurrentUICulture;
            CultureWithName? found = LocalizerSettings.SupportedCultures.FirstOrDefault(x => string.Equals(x.Culture, current.Name, StringComparison.OrdinalIgnoreCase))
                ?? LocalizerSettings.SupportedCultures.FirstOrDefault(x => new CultureInfo(x.Culture).TwoLetterISOLanguageName == current.TwoLetterISOLanguageName);
            return found?.Culture ?? LocalizerSettings.NeutralCulture.Culture;
        }

        /// <summary>Anzeigename einer Sprache, z. B. "Deutsch".</summary>
        public static string DisplayName(string culture) {
            return LocalizerSettings.SupportedCultures.FirstOrDefault(x => x.Culture == culture)?.Name ?? culture;
        }

        /// <summary>Baut die Mail je Sprache mit <paramref name="build"/> (Betreff, Text), fuegt die Sprachen zusammen und oeffnet das Mailprogramm.</summary>
        /// <param name="recipientLanguage">Sprache des Empfaengers; leer = Deutsch.</param>
        public static async Task SendAsync(IJSRuntime js, TextService texts, string to, string? recipientLanguage,
                                            Func<string, (string Subject, string Body)> build) {
            List<string> languages = [Normalize(recipientLanguage), UserLanguage()];
            languages = languages.Distinct().ToList();
            await Task.WhenAll(languages.Select(texts.LoadAsync));
            List<MailPart> parts = languages.Select(lang => {
                (string subject, string body) = build(lang);
                return new MailPart(DisplayName(lang), subject, body);
            }).ToList();
            (string finalSubject, string finalBody) = BilingualMail.Combine(parts);
            await js.InvokeAsync<object>("blazorExtensions.SendLocalEmail", new object[] { to, finalSubject, finalBody });
        }
    }
}
