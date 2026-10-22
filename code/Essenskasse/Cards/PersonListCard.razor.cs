using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Modells;

namespace Essenskasse.Cards {

    public partial class PersonListCard {

        /// <summary>Person, die die Kachel zeigt.</summary>
        [Parameter]
        public Person Person { get; set; } = new Person();

        /// <summary>Saldo liegt ueber dem Dashboard-Grenzwert (gruen), sonst auffaellig (rot).</summary>
        [Parameter]
        public bool PositivSaldo { get; set; } = false;

        private string Initials => PersonText.Initials(Person.Name);

        private async Task CopyGuid() {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Person.Guid.ToString());
        }
    }
}
