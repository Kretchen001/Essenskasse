using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;

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

        /// <summary>Wird aufgerufen, nachdem die Person ausgeblendet wurde.</summary>
        [Parameter]
        public EventCallback<Person> OnHidden { get; set; }

        private bool isEditing = false;
        private bool confirmingHide = false;
        private bool isHiding = false;
        private bool showDetails = false;
        private bool historyLoading = false;
        private bool historyLoaded = false;
        private List<Payment> payments = new List<Payment>();
        private List<Order> orders = new List<Order>();

        private const int HistoryAmount = 5;
        private bool isSaving = false;
        private string editName = "";
        private string editEmail = "";

        private bool NameChanged => editName.Trim() != Person.Name;
        private bool EmailChanged => editEmail.Trim() != Person.Email;
        private bool IsDirty => NameChanged || EmailChanged;

        private string Initials => PersonText.Initials(Person.Name);

        private void AskHide() {
            confirmingHide = !confirmingHide;
        }

        private void CancelHide() {
            confirmingHide = false;
        }

        private async Task ConfirmHide() {
            isHiding = true;
            HttpRequestMessage request = new HttpRequestMessage() {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(Config["API-Adress"] + "person/remove"),
                Headers = { { "guid", Person.Guid.ToString() } }
            };
            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(request);
            }
            catch {
                isHiding = false;
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }
            isHiding = false;

            if (response.IsSuccessStatusCode) {
                confirmingHide = false;
                ToastService.ShowToast(Person.Name + " " + _loc[nameof(RessourceAppStrings.toastPersonHidden)], ToastLevel.Success);
                await OnHidden.InvokeAsync(Person);
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        private async Task ToggleDetails() {
            showDetails = !showDetails;
            if (showDetails && !historyLoaded) {
                await LoadHistory();
            }
        }

        /// <summary>Laedt die letzten Einzahlungen und Bestellungen der Person (einmal je Kachel).</summary>
        private async Task LoadHistory() {
            historyLoading = true;
            try {
                List<Payment>? loadedPayments = await RequestListAsync<Payment>("payment/fromPerson");
                List<Order>? loadedOrders = await RequestListAsync<Order>("order/fromPerson");
                if (loadedPayments is null || loadedOrders is null) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Warning);
                    return; // beim naechsten Aufklappen erneut versuchen
                }
                loadedPayments.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment ?? ""));
                payments = loadedPayments;
                orders = loadedOrders;
                historyLoaded = true;
            }
            finally {
                historyLoading = false;
            }
        }

        private async Task<List<T>?> RequestListAsync<T>(string endpoint) {
            HttpRequestMessage request = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + endpoint),
                Headers = {
                    { "guid", Person.Guid.ToString() },
                    { "amount", HistoryAmount.ToString() }
                }
            };
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) {
                    return null;
                }
                return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync()) ?? new List<T>();
            }
            catch {
                return null;
            }
        }

        private async Task CopyGuid() {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Person.Guid.ToString());
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.copied)], ToastLevel.Success);
        }

        private Task SendPaymentMail() => PaymentMail.SendAsync(JSRuntime, _loc, ToastService, Person);

        private async Task CopyEmail() {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Person.Email);
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
            if (!IsDirty) {
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
