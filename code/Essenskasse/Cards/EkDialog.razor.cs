using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Essenskasse.Cards {

    /// <summary>Allgemeiner Dialog im ek-Stil: Titel, Schliessen-Knopf, Klick auf den Hintergrund und Esc schliessen.
    /// Der Besitzer steuert ueber <see cref="Open"/> und <see cref="OnClose"/>.</summary>
    public partial class EkDialog {

        private static int counter = 0;
        private readonly string dialogId = "ek-dialog-" + System.Threading.Interlocked.Increment(ref counter);
        private string titleId => dialogId + "-title";
        private bool wasOpen = false;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        public bool Open { get; set; }

        [Parameter]
        public string Title { get; set; } = "";

        /// <summary>Breiter Dialog (z. B. fuer Listen).</summary>
        [Parameter]
        public bool Wide { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>Knopfleiste am Ende des Dialogs.</summary>
        [Parameter]
        public RenderFragment? Footer { get; set; }

        private async Task CloseAsync() {
            await OnClose.InvokeAsync();
        }

        private async Task OnKeyDown(KeyboardEventArgs e) {
            if (e.Key == "Escape") {
                await CloseAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            // Beim Oeffnen den Fokus in den Dialog setzen (Tastaturbedienung und Esc)
            if (Open && !wasOpen) {
                await JSRuntime.InvokeVoidAsync("ekFocus", dialogId);
            }
            wasOpen = Open;
        }
    }
}
