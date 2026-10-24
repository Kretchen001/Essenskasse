using Essenskasse.Client.Localization;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Modells;

namespace Essenskasse.Client {

    /// <summary>Baut die Zahlungsaufforderung (bzw. Kontostandsmeldung) und oeffnet das lokale Mailprogramm.</summary>
    public static class PaymentMail {

        /// <summary>Betreff und Text der Mail fuer die Person in einer Sprache; bei positivem Saldo eine reine Information.
        /// Der Text ist je Sprache ein ganzer Text mit den Platzhaltern {0} Name und {1} Saldo, damit die Uebersetzung
        /// die Wortstellung frei waehlen kann.</summary>
        public static (string Subject, string Body) Build(TextService texts, string language, Person person) {
            bool positive = person.Saldo.IsPositiv();
            string subject = texts.Get(language, positive ? nameof(RessourceAppStrings.emailSubjectPositiv) : nameof(RessourceAppStrings.emailSubject));
            string template = texts.Get(language, positive ? nameof(RessourceAppStrings.emailBodyPositive) : nameof(RessourceAppStrings.emailBodyNegative));
            return (subject, string.Format(template, person.Name, person.Saldo.ToString()));
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
