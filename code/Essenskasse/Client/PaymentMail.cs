using Essenskasse.Client.Localization;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Modells;

namespace Essenskasse.Client {

    /// <summary>Baut die Zahlungsaufforderung (bzw. Kontostandsmeldung) und oeffnet das lokale Mailprogramm.</summary>
    public static class PaymentMail {

        /// <summary>Betreff und Text der Mail fuer die Person in einer Sprache; bei positivem Saldo eine reine Information.</summary>
        public static (string Subject, string Body) Build(TextService texts, string language, Person person) {
            string T(string key) => texts.Get(language, key);
            string subject = T(nameof(RessourceAppStrings.emailSubject));
            string body = T(nameof(RessourceAppStrings.emailDearSirMadam)) + " " + person.Name + ",\n\n" +
                            T(nameof(RessourceAppStrings.emailYouAreCurrently)) + "\n" +
                            "\xA0\xA0\xA0\xA0" + person.Saldo.ToString() + "\n";
            if (person.Saldo.IsPositiv()) {
                subject = T(nameof(RessourceAppStrings.emailSubjectPositiv));
                body += T(nameof(RessourceAppStrings.emailPositivButNotNegativ));
            }
            else {
                body += T(nameof(RessourceAppStrings.emailPositiv)) + "\n\n" +
                            T(nameof(RessourceAppStrings.emailRemedy));
            }
            body += "\n\n" + T(nameof(RessourceAppStrings.emailRemaining)) + "\n" +
                            T(nameof(RessourceAppStrings.emailCashManagement));
            return (subject, body);
        }

        /// <summary>Oeffnet das Mailprogramm mit der vorbereiteten Mail (Sprache der Person, danach die des Benutzers). Ohne E-Mail-Adresse kommt ein Hinweis.</summary>
        public static async Task SendAsync(IJSRuntime js, TextService texts, IStringLocalizer<App> loc, ToastService toast, Person person) {
            if (string.IsNullOrWhiteSpace(person.Email)) {
                toast.ShowToast(loc[nameof(RessourceAppStrings.noEmailEntered)], ToastLevel.Info);
                return;
            }
            await MailSender.SendAsync(js, texts, person.Email, person.Language, lang => Build(texts, lang, person));
        }
    }
}
