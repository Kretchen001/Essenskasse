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

        private string Initials => PersonText.Initials(Person.Name);

        private Task SendEmailForPayment() => PaymentMail.SendAsync(JSRuntime, _loc, ToastService, Person);
    }
}
