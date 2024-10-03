using Essenskasse.Cards;
using Essenskasse.Client;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    public partial class ChangeEntitiesPage {

        private bool Loaded = false;
        private bool noMorePaymentsToLoad = false;
        private bool noMoreOrderToLoad = false;
        private int OpacityOfNoMorePaymentToLoad {
            get {
                return noMorePaymentsToLoad ? 50 : 100;
            }
        }
        private int OpacityOfNoMoreOrderToLoad {
            get {
                return noMoreOrderToLoad ? 50 : 100;
            }
        }

        List<string> types = [];
        string SelectedType { get; set; } = string.Empty;

        List<Person> PersonList { get; set; } = [];
        List<Provider> Providers { get; set; } = [];
        List<Payment> Payments { get; set; } = [];
        List<Order> Orders { get; set; } = [];

        List<Payment> ChangedPayments { get; set; } = [];
        List<decimal> PaymentAmount { get; set; } = [];

        List<Order> ChangedOrders { get; set; } = [];
        List<decimal> OrderAmount { get; set; } = [];

        private HistoryModalCard? ModalDialogP { get; set; }
        private HistoryModalCard? ModalDialogO { get; set; }

        private decimal newMoneyAmount = 0;
        private string newComment = "";
        private Guid newSelectedProvider = Guid.Empty;
        private int currentPaymentChangeIndex = 0;
        private int currentOrderChangeIndex = 0;

        protected override async Task OnInitializedAsync() {
            if (!AuthStateService.IsAuthenticated) {
                Nav.NavigateTo("/");
            }
            else {
                types.Add(_loc[nameof(RessourceAppStrings.payment)]);
                types.Add(_loc[nameof(RessourceAppStrings.order)]);
                await RequestPerson();
                await RequestProvider();
                await RequestPayments();
                await RequestOrders();
                SelectedType = types[0];
                Loaded = true;
            }
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
                    Orders.Add(orderS[i]);
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

        private async Task CommitChangesP() {
            ModalDialogP!.Close();
            long euro = (long) newMoneyAmount;
            short cent = (short)((newMoneyAmount - euro) * 100);
            Payment newPayment = new Payment() {
                DateTime = Payments[currentPaymentChangeIndex].DateTime,
                ReferencedPerson = Payments[currentPaymentChangeIndex].ReferencedPerson,
                Amount = new Money(euro, cent),
                Comment = newComment
            };
            if (Payments[currentPaymentChangeIndex].Amount.Equals(newPayment.Amount) &&
                Payments[currentPaymentChangeIndex].Comment.Equals(newPayment.Comment)) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoChanges)], ToastLevel.Info);
                return;
            }
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "payment/update"),
                Headers = {
                    { "accessToken", AppData.AccessToken.ToString() },
                    { "dateAsLong", newPayment.DateTime.Ticks.ToString() }
                },
                Content = new StringContent(JsonConvert.SerializeObject(newPayment.Amount), Encoding.UTF8, "application/json")
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
                Payments[currentPaymentChangeIndex] = newPayment;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }

            // reset temps
            newMoneyAmount = 0;
            newComment = "";
            base.StateHasChanged();
            return;
        }

        private async Task CommitChangesO() {
            ModalDialogO!.Close();
            long euro = (long)newMoneyAmount;
            short cent = (short)((newMoneyAmount - euro) * 100);
            Order newOrder = new Order() {
                DateTime = Orders[currentOrderChangeIndex].DateTime,
                Money = new Money(euro, cent),
                ReferencedPerson = Orders[currentOrderChangeIndex].ReferencedPerson
            };
            if (!newSelectedProvider.Equals(Guid.Empty) && Providers.Exists(x => x.Guid.Equals(newSelectedProvider))) {
                newOrder.Provider = Providers.First(x => x.Guid.Equals(newSelectedProvider));
            }
            else {
                newOrder.Provider = Orders[currentOrderChangeIndex].Provider;
            }
            if (Orders[currentOrderChangeIndex].Equals(newOrder)) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoChanges)], ToastLevel.Info);
                return;
            }
            MoneyProviderGuidData data = new MoneyProviderGuidData() {
                Money = newOrder.Money,
                Provider = newOrder.Provider,
                ReferencedPerson = newOrder.ReferencedPerson
            };
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "order/update"),
                Headers = {
                    { "accessToken", AppData.AccessToken.ToString() },
                    { "dateAsLong", newOrder.DateTime.Ticks.ToString() }
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
                Orders[currentOrderChangeIndex] = newOrder;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }

            // reset temps
            newMoneyAmount = 0;
            newComment = "";
            newSelectedProvider = Guid.Empty;
            base.StateHasChanged();
            return;
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
                if (getted.Count == 0) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNothingToLoad)], ToastLevel.Info);
                    noMorePaymentsToLoad = true;
                }
                else {
                    getted.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment));
                    Payments.AddRange(getted);
                    ChangedPayments.AddRange(getted);
                    Payments.ForEach(x => {
                        decimal temp = x.Amount.Euro + (decimal)(x.Amount.Cent * 0.01);
                        PaymentAmount.Add(temp);
                    });
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
            base.StateHasChanged();
        }

        private async void LoadNextAmountOfOrders() {
            // Request last payments of a person
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "order/requestNextAmount"),
                Headers = {
                        { "amount", AppData.LoadAmountOrder.ToString() },
                        { "alreadyLoaded", Orders.Count.ToString()}
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
                List<Order> getted = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? [];
                if (getted.Count == 0) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNothingToLoad)], ToastLevel.Info);
                    noMoreOrderToLoad = true;
                }
                else {
                    foreach (Order x in getted) {
                        Orders.Add(x);
                        ChangedOrders.Add(x);
                        decimal temp = x.Money.Euro + (decimal)(x.Money.Cent * 0.01);
                        OrderAmount.Add(temp);
                    }
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }

            base.StateHasChanged();
        }

        private void ShowPaymentChangeModal(int index) {
            currentPaymentChangeIndex = index;
            ModalDialogP!.Open();
        }

        private void ShowOrderChangeModal(int index) {
            currentOrderChangeIndex = index;
            ModalDialogO!.Open();
        }
    }
}
