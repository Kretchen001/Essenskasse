using Essenskasse.Client;
using Microsoft.AspNetCore.Components;
using Modells;

namespace Essenskasse.Cards {

    /// <summary>Kachel zum Auswaehlen einer Person (Name und Stand, Farbe wie im Dashboard).</summary>
    public partial class PersonPickCard {

        [Parameter]
        public Person Person { get; set; } = new Person();

        /// <summary>Saldo liegt ueber dem Dashboard-Grenzwert (gruen), sonst auffaellig (rot).</summary>
        [Parameter]
        public bool PositivSaldo { get; set; } = false;

        [Parameter]
        public EventCallback<Person> OnPick { get; set; }

        private Task Pick() => OnPick.InvokeAsync(Person);
    }
}
