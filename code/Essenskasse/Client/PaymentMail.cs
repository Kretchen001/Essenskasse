using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Modells;

namespace Essenskasse.Client {

    /// <summary>Baut die Zahlungsaufforderung (bzw. Kontostandsmeldung) und oeffnet das lokale Mailprogramm.</summary>
    public static class PaymentMail {

        /// <summary>Betreff und Text der Mail fuer die Person; bei positivem Saldo eine reine Information.</summary>
        public static (string Subject, string Body) Build(IStringLocalizer<App> loc, Person person) {
            string subject = loc[nameof(RessourceAppStrings.emailSubject)];
            string body = loc[nameof(RessourceAppStrings.emailDearSirMadam)] + " " + person.Name + ",\n\n" +
                            loc[nameof(RessourceAppStrings.emailYouAreCurrently)] + "\n" +
                            "\xA0\xA0\xA0\xA0" + person.Saldo.ToString() + "\n";
            if (person.Saldo.IsPositiv()) {
                subject = loc[nameof(RessourceAppStrings.emailSubjectPositiv)];
                body += loc[nameof(RessourceAppStrings.emailPositivButNotNegativ)] + "\n\n" +
                            loc[nameof(RessourceAppStrings.emailRemaining)] + "\n" +
                            loc[nameof(RessourceAppStrings.emailCashManagement)];
            }
            else {
                body += loc[nameof(RessourceAppStrings.emailPositiv)] + "\n" +
                            loc[nameof(RessourceAppStrings.emailRemedy)] + "\n\n" +
                            loc[nameof(RessourceAppStrings.emailRemaining)] + "\n" +
                            loc[nameof(RessourceAppStrings.emailCashManagement)];
            }
            return (subject, body);
        }

        /// <summary>Oeffnet das Mailprogramm mit der vorbereiteten Mail. Ohne E-Mail-Adresse kommt ein Hinweis.</summary>
        public static async Task SendAsync(IJSRuntime js, IStringLocalizer<App> loc, ToastService toast, Person person) {
            if (string.IsNullOrWhiteSpace(person.Email)) {
                toast.ShowToast(loc[nameof(RessourceAppStrings.noEmailEntered)], ToastLevel.Info);
                return;
            }
            (string subject, string body) = Build(loc, person);
            await js.InvokeAsync<object>("blazorExtensions.SendLocalEmail", new object[] { person.Email, subject, body });
        }
    }
}
