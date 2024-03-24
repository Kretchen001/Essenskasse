using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {
    
    public partial class ChangeEntitiesPage {

        readonly List<string> types = ["Einzahlungen", "Bestellungen"];
        string SelectedType { get; set; } = string.Empty;

        List<Person> PersonList { get; set; } = [];
        List<Provider> Providers { get; set; } = [];
        List<Payment> Payments { get; set; } = [];
        List<Tuple<Order, int>> Orders { get; set; } = [];

        List<Payment> ChangedPayments { get; set; } = [];
        List<decimal> PaymentAmount { get; set; } = [];

        List<Order> ChangedOrders { get; set; } = [];
        List<decimal> OrderAmount { get; set; } = [];

        protected override async Task OnInitializedAsync() {
            await requestPerson();
            await requestProvider();
            await requestPayments();
            await requestOrders();
            SelectedType = types[0];
            base.StateHasChanged();
        }

        private async Task requestPerson() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/requestAll")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                PersonList = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                PersonList.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                PersonList.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                PersonList = PersonList.OrderBy(x => x.Name).ToList();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async Task requestProvider() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "provider/requestAll")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) { // success?
                                                // deserialize the result-JSON with Newtownsoft to the List
                Providers = JsonConvert.DeserializeObject<List<Provider>>(await response.Content.ReadAsStringAsync()) ?? new List<Provider>();
                Providers.ForEach(x => x.Designation = Uri.UnescapeDataString(x.Designation));
                Providers = Providers.OrderBy(x => x.Designation).ToList();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async Task requestPayments() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "payment/requestAmount"),
                Headers = {
                    { "amount", AppData.LoadAmountPayment.ToString() }
                }
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                Payments = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();
                ChangedPayments = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();

                Payments.ForEach(x => {
                    long longTemp = x.Amount.Euro;
                    longTemp += (long)(x.Amount.Cent * 0.01);
                    PaymentAmount.Add(longTemp);
                });
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        private async Task requestOrders() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "order/requestAmount"),
                Headers = {
                    { "amount", AppData.LoadAmountPayment.ToString() }
                }
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                List<Order> orderS = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? new List<Order>();
                ChangedOrders = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? new List<Order>();
                for (int i = 0; i < orderS.Count; i += 1) {
                    long longTemp = ChangedOrders[i].Money.Euro;
                    longTemp += (long)(ChangedOrders[i].Money.Cent * 0.01);
                    OrderAmount.Add(longTemp);
                    Orders.Add(new Tuple<Order, int>(orderS[i], i));
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        private string getTheDesignationOfPerson(Guid guid) {
            foreach (Person x in PersonList) {
                if (x.Guid.Equals(guid)) {
                    return x.Name;
                }
            }
            return "";
        }

        private async Task commitChangesP(int index) {
            long euro = (long)PaymentAmount[index];
            short cent = (short)((PaymentAmount[index] - euro) * 100);
            ChangedPayments[index].Amount = new Money(euro, cent);
            if (Payments[index].Amount.Equals(ChangedPayments[index].Amount) &&
                Payments[index].DateTime.Equals(ChangedPayments[index].DateTime)) {
                ToastService.ShowToast("Keine Änderungen detektiert!", ToastLevel.Info);
                return;
            }
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "payment/update"),
                Headers = {
                    { "accessToken", AppData.AccessToken.ToString() },
                    { "dateAsLong", ChangedPayments[index].DateTime.Ticks.ToString() }
                },
                Content = new StringContent(JsonConvert.SerializeObject(ChangedPayments[index].Amount), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                ToastService.ShowToast("Update erfolgreich", ToastLevel.Success);
                Payments[index] = new Payment() {
                    ReferencedPerson = Payments[index].ReferencedPerson,
                    DateTime = ChangedPayments[index].DateTime,
                    Amount = ChangedPayments[index].Amount
                };
                base.StateHasChanged();
                return;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }
        }

        private async Task commitChangesO(int index) {
            long euro = (long)OrderAmount[index];
            short cent = (short)((OrderAmount[index] - euro) * 100);
            ChangedOrders[index].Money = new Money(euro, cent);
            if (Orders[index].Item1.Equals(ChangedOrders[index])) {
                ToastService.ShowToast("Keine Änderungen detektiert!", ToastLevel.Info);
                return;
            }
            Money? a = ChangedOrders[index].Money;
            Provider? b = ChangedOrders[index].Provider;
            Guid? c = ChangedOrders[index].ReferencedPerson;
            if (Orders[index].Item1.Money.Equals(ChangedOrders[index].Money)) {
                a = null;
            }
            if (Orders[index].Item1.Provider.Equals(ChangedOrders[index].Provider)) {
                b = null;
            }
            if (Orders[index].Item1.ReferencedPerson.Equals(ChangedOrders[index].ReferencedPerson)) {
                c = null;
            }
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "order/update"),
                Headers = {
                    { "accessToken", AppData.AccessToken.ToString() },
                    { "dateAsLong", ChangedOrders[index].DateTime.Ticks.ToString() }
                },
                Content = new StringContent(JsonConvert.SerializeObject(new Tuple<Money?, Provider?, Guid?>(a, b, c)), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                ToastService.ShowToast("Update erfolgreich", ToastLevel.Success);
                Orders[index] = new Tuple<Order, int>(new Order() {
                    DateTime = ChangedOrders[index].DateTime,
                    Provider = ChangedOrders[index].Provider,
                    Money = ChangedOrders[index].Money,
                    ReferencedPerson = ChangedOrders[index].ReferencedPerson
                },
                index);
                base.StateHasChanged();
                return;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }
        }
    }
}
