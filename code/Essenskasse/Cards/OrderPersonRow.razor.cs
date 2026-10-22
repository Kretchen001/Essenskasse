using Essenskasse.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Modells;

namespace Essenskasse.Cards {

    /// <summary>Eine Personenzeile im Bestellfenster: Name, Betragseingabe und Platz fuer weitere Inhalte (Chips, Verlauf).</summary>
    public partial class OrderPersonRow {

        [Parameter, EditorRequired]
        public Person Person { get; set; } = new Person();

        /// <summary>Der eingetippte Betrag als Text.</summary>
        [Parameter]
        public string Text { get; set; } = "";

        [Parameter]
        public EventCallback<string> TextChanged { get; set; }

        /// <summary>Der Text ist kein gueltiger Betrag.</summary>
        [Parameter]
        public bool Invalid { get; set; }

        /// <summary>Der Saldo der Person liegt ueber dem Grenzwert (gruener Rand), sonst roter Rand.</summary>
        [Parameter]
        public bool SaldoOk { get; set; } = true;

        /// <summary>Eindeutige Id des Eingabefeldes (fuer den Sprung zur naechsten Person).</summary>
        [Parameter]
        public string InputId { get; set; } = "";

        /// <summary>Enter im Betragsfeld.</summary>
        [Parameter]
        public EventCallback OnEnter { get; set; }

        [Parameter]
        public bool HistoryOpen { get; set; }

        [Parameter]
        public EventCallback OnToggleHistory { get; set; }

        /// <summary>Weitere Inhalte unter der Zeile (Chips, Verlauf).</summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        private async Task OnInput(ChangeEventArgs e) {
            await TextChanged.InvokeAsync(e.Value?.ToString() ?? "");
        }

        private async Task OnKey(KeyboardEventArgs e) {
            if (e.Key == "Enter") {
                await OnEnter.InvokeAsync();
            }
        }

        private async Task Clear() {
            await TextChanged.InvokeAsync("");
        }
    }
}
