using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Modells;

namespace Essenskasse.Cards {

    public partial class ProviderListCard {

        /// <summary>Anbieter, den die Kachel zeigt.</summary>
        [Parameter]
        public Provider Provider { get; set; } = new Provider();

        /// <summary>Wird aufgerufen, nachdem die Bezeichnung erfolgreich gespeichert wurde.</summary>
        [Parameter]
        public EventCallback<Provider> OnUpdated { get; set; }

        /// <summary>Wird aufgerufen, nachdem der Anbieter entfernt wurde.</summary>
        [Parameter]
        public EventCallback<Provider> OnRemoved { get; set; }

        private bool isEditing = false;
        private bool confirmingRemove = false;
        private bool isRemoving = false;
        private bool isSaving = false;
        private string editName = "";

        private bool IsDirty => editName.Trim() != Provider.Designation;

        private string Initials => PersonText.Initials(Provider.Designation);

        private async Task CopyGuid() {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Provider.Guid.ToString());
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.copied)], ToastLevel.Success);
        }

        /// <summary>Enter speichert, Esc verwirft die Bearbeitung.</summary>
        private async Task OnEditKeyDown(KeyboardEventArgs e) {
            if (isSaving) {
                return;
            }
            if (e.Key == "Enter" && IsDirty) {
                await SaveEdit();
            }
            else if (e.Key == "Escape") {
                CancelEdit();
            }
        }

        private void AskRemove() {
            confirmingRemove = !confirmingRemove;
        }

        private void CancelRemove() {
            confirmingRemove = false;
        }

        /// <summary>Entfernt den Anbieter aus der Auswahl (die API blendet ihn nur aus, Bestellungen bleiben erhalten).</summary>
        private async Task ConfirmRemove() {
            isRemoving = true;
            HttpRequestMessage request = new HttpRequestMessage() {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(Config["API-Adress"] + "provider/remove"),
                Headers = { { "guid", Provider.Guid.ToString() } }
            };
            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(request);
            }
            catch {
                isRemoving = false;
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }
            isRemoving = false;

            if (response.IsSuccessStatusCode) {
                confirmingRemove = false;
                ToastService.ShowToast(Provider.Designation + " " + _loc[nameof(RessourceAppStrings.toastProviderRemoved)], ToastLevel.Success);
                await OnRemoved.InvokeAsync(Provider);
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoRemove)] + "\n" + _loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }
        }

        private void StartEdit() {
            editName = Provider.Designation;
            isEditing = true;
        }

        private void CancelEdit() {
            isEditing = false;
        }

        private async Task SaveEdit() {
            string name = editName.Trim();
            if (name.Length == 0) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.nameRequired)], ToastLevel.Warning);
                return;
            }
            if (!IsDirty) {
                return;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "provider/update"),
                Headers = {
                    { "guid", Provider.Guid.ToString() },
                    { "designation", Uri.EscapeDataString(name) }
                }
            };

            isSaving = true;
            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                isSaving = false;
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }
            isSaving = false;

            if (response.IsSuccessStatusCode) {
                Provider.Designation = name;
                isEditing = false;
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastUpdateSuccessful)], ToastLevel.Success);
                await OnUpdated.InvokeAsync(Provider);
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSaveError)], ToastLevel.Warning);
            }
        }
    }
}
