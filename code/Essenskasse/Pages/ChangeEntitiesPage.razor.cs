using Essenskasse.Client;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    public partial class ChangeEntitiesPage {

        List<string> types = [];
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
            types.Add(_loc[nameof(RessourceAppStrings.payment)]);
            types.Add(_loc[nameof(RessourceAppStrings.order)]);
            await RequestPerson();
            await RequestProvider();
            await RequestPayments();
            await RequestOrders();
            SelectedType = types[0];
            base.StateHasChanged();
        }

        private async Task RequestPerson() {
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
                PersonList = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                PersonList.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                PersonList.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                PersonList = PersonList.OrderBy(x => x.Name).ToList();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async Task RequestProvider() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "provider/requestAll")
            };

            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

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

        private async Task RequestPayments() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "payment/requestAmount"),
                Headers = {
                    { "amount", AppData.LoadAmountPayment.ToString() }
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
                Payments = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();
                ChangedPayments = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();

                Payments.ForEach(x => {
                    decimal temp = x.Amount.Euro + (decimal)(x.Amount.Cent * 0.01);
                    PaymentAmount.Add(temp);
                });
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        private async Task RequestOrders() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "order/requestAmount"),
                Headers = {
                    { "amount", AppData.LoadAmountOrder.ToString() }
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
                List<Order> orderS = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? new List<Order>();
                ChangedOrders = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? new List<Order>();
                for (int i = 0; i < orderS.Count; i += 1) {
                    decimal temp = ChangedOrders[i].Money.Euro + (decimal)(ChangedOrders[i].Money.Cent * 0.01);
                    OrderAmount.Add(temp);
                    Orders.Add(new Tuple<Order, int>(orderS[i], i));
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        private string GetTheDesignationOfPerson(Guid guid) {
            foreach (Person x in PersonList) {
                if (x.Guid.Equals(guid)) {
                    return x.Name;
                }
            }
            return "";
        }

        private async Task CommitChangesP(int index) {
            long euro = (long)PaymentAmount[index];
            short cent = (short)((PaymentAmount[index] - euro) * 100);
            ChangedPayments[index].Amount = new Money(euro, cent);
            if (Payments[index].Amount.Equals(ChangedPayments[index].Amount) &&
                Payments[index].DateTime.Equals(ChangedPayments[index].DateTime)) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoChanges)], ToastLevel.Info);
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

            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

            if (response.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastUpdateSuccessful)], ToastLevel.Success);
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

        private async Task CommitChangesO(int index) {
            long euro = (long)OrderAmount[index];
            short cent = (short)((OrderAmount[index] - euro) * 100);
            ChangedOrders[index].Money = new Money(euro, cent);
            if (Orders[index].Item1.Equals(ChangedOrders[index])) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoChanges)], ToastLevel.Info);
                return;
            }
            Money? a = ChangedOrders[index].Money;
            Provider? b = ChangedOrders[index].Provider;
            Guid? c = ChangedOrders[index].ReferencedPerson;
            if (Orders[index].Item1.Money.Equals(ChangedOrders[index].Money)) {
                a = null;
            }
            if (Orders[index].Item1.Provider.Equals(ChangedOrders[index].Provider) || Providers.Exists(x => x.Guid.Equals(ChangedOrders[index].Provider.Guid))) {
                b = null;
            }
            if (Orders[index].Item1.ReferencedPerson.Equals(ChangedOrders[index].ReferencedPerson)) {
                c = null;
            }
            MoneyProviderGuidData data = new MoneyProviderGuidData() { 
                Money = a,
                Provider = b,
                ReferencedPerson = c
            };
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "order/update"),
                Headers = {
                    { "accessToken", AppData.AccessToken.ToString() },
                    { "dateAsLong", ChangedOrders[index].DateTime.Ticks.ToString() },
                    { "referencedPerson", c.ToString() }
                },
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
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
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastUpdateSuccessful)], ToastLevel.Success);
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

        private string ShowMoney(int index) {
            long euro = 0; short cent = 0;
            if (SelectedType == types[0]) {
                euro = (long)PaymentAmount[index];
                cent = (short)((PaymentAmount[index] - euro) * 100);
            }
            else if (SelectedType == types[1]) {
                euro = (long)OrderAmount[index];
                cent = (short)((OrderAmount[index] - euro) * 100);
            }
            return new Money(euro, cent).ToString();
        }

        private async void LoadNextAmountOfPayments() {
            // Request last payments of a person
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "payment/requestNextAmount"),
                Headers = {
                    { "amount", AppData.LoadAmountPayment.ToString() },
                    { "alreadyLoaded", Payments.Count.ToString()}
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
                List<Payment> getted = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();
                getted.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment));
                Payments.AddRange(getted);
                ChangedPayments.AddRange(getted);
                Payments.ForEach(x => {
                    decimal temp = x.Amount.Euro + (decimal)(x.Amount.Cent * 0.01);
                    PaymentAmount.Add(temp);
                });
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }

            base.StateHasChanged();
        }
    }
}
