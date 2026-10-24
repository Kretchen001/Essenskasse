namespace Essenskasse.Layout {

    public partial class NavMenu {

        private bool collapseNavMenu = true;

        private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        private void ToggleNavMenu() {
            collapseNavMenu = !collapseNavMenu;
        }

        /// <summary></summary>
        /// <returns></returns>
        public async Task ReloadNavMenu() {
            base.StateHasChanged();
        }
    }
}
