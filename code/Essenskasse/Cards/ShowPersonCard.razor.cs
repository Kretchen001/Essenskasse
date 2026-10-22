using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;

namespace Essenskasse.Cards {

    public partial class ShowPersonCard {

        /// <summary>Person, that the Card must show.</summary>
        [Parameter]
        public Person Person { get; set; } = new Person();

        /// <summary>Status of the Person's saldo.</summary>
        [Parameter]
        public bool PositivSaldo { get; set; } = false;

        /// <summary>Anfangsbuchstaben fuer den Kreis neben dem Namen (Vor- und Nachname, sonst nur der erste).</summary>
        private string Initials {
            get {
                string[] parts = (Person.Name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) {
                    return "?";
                }
                if (parts.Length == 1) {
                    return char.ToUpper(parts[0][0]).ToString();
                }
                return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
            }
        }

        private async void SendEmailForPayment() {
            if (Person.Email.Equals("")) {
                ToastService.ShowToast("Keine E-Mail eingetragen!", ToastLevel.Info);
            }
            else {
                string subject = _loc[nameof(RessourceAppStrings.emailSubject)];
                string body = _loc[nameof(RessourceAppStrings.emailDearSirMadam)] + " " + Person.Name + ",\n\n" +
                                _loc[nameof(RessourceAppStrings.emailYouAreCurrently)] + "\n" +
                                "\xA0\xA0\xA0\xA0" + Person.Saldo.ToString() + "\n";
                if (Person.Saldo.IsPositiv()) {
                    subject = _loc[nameof(RessourceAppStrings.emailSubjectPositiv)]; // change the subject to Information when positiv Saldo
                    body += _loc[nameof(RessourceAppStrings.emailPositivButNotNegativ)] + "\n\n" +
                                _loc[nameof(RessourceAppStrings.emailRemaining)] + "\n" +
                                _loc[nameof(RessourceAppStrings.emailCashManagement)];
                }
                else {
                    body += _loc[nameof(RessourceAppStrings.emailPositiv)] + "\n" +
                                _loc[nameof(RessourceAppStrings.emailRemedy)] + "\n\n" +
                                _loc[nameof(RessourceAppStrings.emailRemaining)] + "\n" +
                                _loc[nameof(RessourceAppStrings.emailCashManagement)];
                }
                await JSRuntime.InvokeAsync<object>(
                    "blazorExtensions.SendLocalEmail",
                    new object[] {
                        Person.Email,
                        subject,
                        body
                    }
                );
            }
        }
    }
}
