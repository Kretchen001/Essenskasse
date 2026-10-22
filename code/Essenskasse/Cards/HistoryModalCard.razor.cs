using Microsoft.AspNetCore.Components;

namespace Essenskasse.Cards {

    public partial class HistoryModalCard {

        [Parameter]
        public string Title { get; set; } = "";

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        private string modalDisplay = "none;";
        private string modalClass = string.Empty;
        private bool showBackdrop = false;

        public void Open () {
            modalDisplay = "block";
            modalClass = "show";
            showBackdrop = true;
        }

        public void Close () {
            modalDisplay = "none";
            modalClass = string.Empty;
            showBackdrop = false;
        }
    }
}
