using Essenskasse.Client;
using Essenskasse.Ressources;

namespace Essenskasse.Cards {

    public partial class ToastCard {

        private string? _heading;
        private string? _message;
        private bool _isVisible;
        private ToastLevel _level = ToastLevel.Info;
        private string _levelCssClass = "ek-toast-info";

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
            _level = level;
            switch (level) {
                case ToastLevel.Info:
                    _levelCssClass = "ek-toast-info";
                    _heading = _loc[nameof(RessourceAppStrings.information)];
                    break;
                case ToastLevel.Success:
                    _levelCssClass = "ek-toast-success";
                    _heading = _loc[nameof(RessourceAppStrings.success)];
                    break;
                case ToastLevel.Warning:
                    _levelCssClass = "ek-toast-warning";
                    _heading = _loc[nameof(RessourceAppStrings.warning)];
                    break;
                case ToastLevel.Error:
                    _levelCssClass = "ek-toast-error";
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
