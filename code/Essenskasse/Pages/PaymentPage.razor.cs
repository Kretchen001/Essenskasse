using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {
    
    public partial class PaymentPage {

        private List<Person>? person = new List<Person>();
        private Guid _selectedPerson;
        private Guid SelectedPerson {
            get {
                return _selectedPerson;
            }
            set {
                _selectedPerson = value;
                RequestPaymentsOfPerson();
            }
        }

        private decimal _Payment = 0;
        private decimal Payment {
            get {
                return _Payment;
            }
            set {
                if (value != 0) {
                    Money temp = ConvertToMoney(value);
                    if (temp.Cent < 100) {
                        _Payment = value;
                    }
                    else {
                        // nothing
                    }
                }
            }
        }
        private Money Money {
            get {
                return ConvertToMoney(_Payment);
            }
        }

        private string comment = "";

        private List<Payment> paymentsOfSelectedPerson = new List<Payment>();
        private int Amount {
            get => AppData.LoadAmountPayment;
        }

        protected override async Task OnInitializedAsync() {
            await RefreshPersonList();
        }

        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Money ConvertToMoney(decimal value) {
            long euro = (long)value;
            short cent = (short)((value - euro) * 100);
            return new Money(euro, cent);
        }

        private async Task RefreshPersonList() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/requestAll")
            };

            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

            if (response.IsSuccessStatusCode) {
                person = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                person.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                person.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                person = person.OrderBy(x => x.Name).ToList();

                if (person.Count > 0) {
                    SelectedPerson = person[0].Guid;
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async void RequestPaymentsOfPerson() {
            // Request last 10 payments of a person
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "payment/fromPerson"),
                Headers = {
                    { "guid", SelectedPerson.ToString() },
                    { "amount", Amount.ToString() }
                }
            };

            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

            if (response.IsSuccessStatusCode) {
                paymentsOfSelectedPerson = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();
                paymentsOfSelectedPerson.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment));
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }

            base.StateHasChanged();
        }

        private async void InsertPayment() {
            if (!(await JSRuntime.InvokeAsync<bool>("confirm", _loc[nameof(RessourceAppStrings.insertFollowingAmount)] + "\n" + Money.ToString()))) {
                return;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "payment/insertNew"),
                Headers = {
                    {"referencedPerson", SelectedPerson.ToString()},
                    {"comment", Uri.EscapeDataString(comment)}
                },
                Content = new StringContent(JsonConvert.SerializeObject(Money), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

            if (response.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSuccessfullyAdded)], ToastLevel.Success);
                paymentsOfSelectedPerson.Add(JsonConvert.DeserializeObject<Payment>(await response.Content.ReadAsStringAsync())!); 
                paymentsOfSelectedPerson.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment));
                paymentsOfSelectedPerson = paymentsOfSelectedPerson.OrderByDescending(x => x.DateTime).ToList();
                base.StateHasChanged();
                return;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNotAdded)] + "\n" + _loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }
        }

        private async Task LoadNextAmountOfPayments() {

        }
    }
}
