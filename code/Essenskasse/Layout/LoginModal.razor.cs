using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Essenskasse.Layout {

    /// <summary>Dialog-Huelle fuer die Anmeldung: Titel, Schliessen-Knopf, Klick auf den Hintergrund und Esc schliessen.</summary>
    public partial class LoginModal {

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>Ueberschrift des Dialogs.</summary>
        [Parameter]
        public string Title { get; set; } = "";

        /// <summary>Id des Feldes, das beim Oeffnen den Fokus bekommt.</summary>
        [Parameter]
        public string FocusElementId { get; set; } = "";

        private bool isOpen = false;
        private bool focusPending = false;

        public void Open() {
            isOpen = true;
            focusPending = true;
            StateHasChanged();
        }

        public void Close() {
            isOpen = false;
            StateHasChanged();
        }

        /// <summary>Setzt den Fokus in das Feld mit der angegebenen Id.</summary>
        public async Task FocusAsync(string elementId) {
            await JSRuntime.InvokeVoidAsync("ekFocus", elementId);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (focusPending && isOpen && FocusElementId.Length > 0) {
                focusPending = false;
                await FocusAsync(FocusElementId);
            }
        }

        private void OnKeyDown(KeyboardEventArgs e) {
            if (e.Key == "Escape") {
                Close();
            }
        }
    }
}
