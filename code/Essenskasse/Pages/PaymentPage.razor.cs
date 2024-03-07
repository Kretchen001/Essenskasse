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
        private Guid selectedPerson {
            get {
                return _selectedPerson;
            }
            set {
                _selectedPerson = value;
                requestPaymentsOfPerson();
            }
        }

        private decimal _payment = 0;
        private decimal payment {
            get {
                return _payment;
            }
            set {
                if (value != 0) {
                    Money temp = convertToMoney(value);
                    if (temp.Cent < 100) {
                        _payment = value;
                    }
                    else {
                        // nothing
                    }
                }
            }
        }
        private Money money {
            get {
                return convertToMoney(_payment);
            }
        }

        private string comment = "";

        private List<Payment> paymentsOfSelectedPerson = new List<Payment>();
        private int amount {
            get => AppData.LoadAmountPayment;
        }

        protected override async Task OnInitializedAsync() {
            await refreshPersonList();
        }

        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private Money convertToMoney(decimal value) {
            long euro = (long)value;
            short cent = (short)((value - euro) * 100);
            return new Money(euro, cent);
        }

        private async Task refreshPersonList() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/requestAll")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                person = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                person.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                person.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                person = person.OrderBy(x => x.Name).ToList();

                if (person.Count > 0) {
                    selectedPerson = person[0].Guid;
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async void requestPaymentsOfPerson() {
            // Request last 10 payments of a person
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "payment/fromPerson"),
                Headers = {
                    { "guid", selectedPerson.ToString() },
                    { "amount", amount.ToString() }
                }
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                paymentsOfSelectedPerson = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }

            base.StateHasChanged();
        }

        private async void insertPayment() {
            if (!(await JSRuntime.InvokeAsync<bool>("confirm", _loc[nameof(RessourceAppStrings.insertFollowingAmount)] + "\n" + money.ToString()))) {
                return;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "payment/insertNew"),
                Headers = {
                    {"referencedPerson", selectedPerson.ToString()},
                    {"comment", comment}
                },
                Content = new StringContent(JsonConvert.SerializeObject(money), Encoding.UTF8, "application/json")
            };

            var response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSuccessfullyAdded)], ToastLevel.Success);
                paymentsOfSelectedPerson.Add(JsonConvert.DeserializeObject<Payment>(await response.Content.ReadAsStringAsync())!);
                base.StateHasChanged();
                return;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNotAdded)] + "\n" + _loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }
        }
    }
}
