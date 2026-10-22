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

        /// <summary>Wird aufgerufen, nachdem Name oder E-Mail erfolgreich gespeichert wurden.</summary>
        [Parameter]
        public EventCallback<Person> OnUpdated { get; set; }

        private bool isEditing = false;
        private bool isSaving = false;
        private string editName = "";
        private string editEmail = "";

        private string Initials => PersonText.Initials(Person.Name);

        private async Task CopyGuid() {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Person.Guid.ToString());
        }

        private void StartEdit() {
            editName = Person.Name;
            editEmail = Person.Email;
            isEditing = true;
        }

        private void CancelEdit() {
            isEditing = false;
        }

        private async Task SaveEdit() {
            string name = editName.Trim();
            string email = editEmail.Trim();
            if (name.Length == 0) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.nameRequired)], ToastLevel.Warning);
                return;
            }
            if (name == Person.Name && email == Person.Email) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoChanges)], ToastLevel.Info);
                isEditing = false;
                return;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "person/update"),
                Headers = {
                    { "guid", Person.Guid.ToString() },
                    { "name", Uri.EscapeDataString(name) },
                    { "email", Uri.EscapeDataString(email) }
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
                Person.Name = name;
                Person.Email = email;
                isEditing = false;
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastUpdateSuccessful)], ToastLevel.Success);
                await OnUpdated.InvokeAsync(Person);
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSaveError)], ToastLevel.Warning);
            }
        }
    }
}
