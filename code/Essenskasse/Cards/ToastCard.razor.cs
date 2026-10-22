using Essenskasse.Client;
using Essenskasse.Ressources;

namespace Essenskasse.Cards {

    public partial class ToastCard {

        private string? _heading;
        private string? _message;
        private bool _isVisible;
        private string? _backgroundCssClass;
        private string? _iconCssClass;

        protected override void OnInitialized () {
            ToastService.OnShow += ShowToast;
            ToastService.OnHide += HideToast;
        }

        private void ShowToast (string message, ToastLevel level) {
            BuildToastSettings(level, message);
            _isVisible = true;
            StateHasChanged();
        }

        private void HideToast () {
            _isVisible = false;
            StateHasChanged();
        }

        private void BuildToastSettings (ToastLevel level, string message) {
            switch (level) {
                case ToastLevel.Info:
                    _backgroundCssClass = $"bg-info";
                    _iconCssClass = "info";
                    _heading = _loc[nameof(RessourceAppStrings.information)];
                    break;
                case ToastLevel.Success:
                    _backgroundCssClass = $"bg-success";
                    _iconCssClass = "check";
                    _heading = _loc[nameof(RessourceAppStrings.success)];
                    break;
                case ToastLevel.Warning:
                    _backgroundCssClass = $"bg-warning";
                    _iconCssClass = "exclamation";
                    _heading = _loc[nameof(RessourceAppStrings.warning)];
                    break;
                case ToastLevel.Error:
                    _backgroundCssClass = "bg-danger";
                    _iconCssClass = "times";
                    _heading = _loc[nameof(RessourceAppStrings.error)];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            _message = message;
        }

        void IDisposable.Dispose () {
            ToastService.OnShow -= ShowToast;
            ToastService.OnHide -= HideToast;
        }
    }
}
