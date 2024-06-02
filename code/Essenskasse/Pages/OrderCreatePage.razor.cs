using Essenskasse.Cards;
using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    public partial class OrderCreatePage {

        private HistoryModalCard? ModalDialog { get; set; }

        private List<Provider> providers = [];
        private List<Person> person = [];
        private List<Money> money = [];
        private List<List<Order>> shownOrders = new List<List<Order>>();
        private List<Order> completeHistoryFromOne = [];
        private List<PersonOrderCard> personOrderCards = [];

        private record DateAndOrder(DateTime DateTime, List<Order?> Orders);
        private List<DateAndOrder> orderListForOverview = [];
        private DateTime referenceDate = DateTime.Now;

        private string ModalTitle = "";

        private DateTime selectedDate = DateTime.Now;

        private Guid selectedProvider = new Guid();

        private List<bool> showHistory = new List<bool>();

        private int Amount {
            get => AppData.LoadAmountOrder;
        }

        protected override async Task OnInitializedAsync() {
            await RequestAllProvider();
            await RequestAllPerson();
            base.StateHasChanged();
        }

        private async Task RequestAllProvider() {
            // Request all providers, that are saved in the database.
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "provider/requestAll")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) { // success?
                                                // deserialize the result-JSON with Newtownsoft to the List
                providers = JsonConvert.DeserializeObject<List<Provider>>(await response.Content.ReadAsStringAsync()) ?? [];
                providers.ForEach(x => x.Designation = Uri.UnescapeDataString(x.Designation));
                providers = providers.OrderBy(x => x.Designation).ToList();
                if (providers.Count > 0) {
                    selectedProvider = providers[0].Guid;
                }
                else {
                    ToastService.ShowToast("Keine Anbieter verfügbar!\nBitte vorher eintragen!", ToastLevel.Info);
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async Task RequestAllPerson() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/requestAll")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                person.Clear();
                person = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                person.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                person.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                person = person.OrderBy(x => x.Name).ToList();
                foreach (Person x in person) {
                    money.Add(new Money());
                    showHistory.Add(false);
                    shownOrders.Add(new List<Order>());
                    personOrderCards.Add(new PersonOrderCard());
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        private void HandleValuesChanged(int index, Money changedValues) {
            // Access to the changed Money
            Money newMoney = changedValues;

            // update List
            money[index] = newMoney;
        }

        private async Task PostOrderForTheDay() {
            // request Date
            HttpRequestMessage httpRequestMessageDate = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "order/requestDate"),
                Headers = {
                    { "dateString", selectedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffK") }
                }
            };

            HttpResponseMessage responseDate = await HttpClient.SendAsync(httpRequestMessageDate);

            int res = 0;
            if (!responseDate.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
            else {
                res = Int32.Parse(await responseDate.Content.ReadAsStringAsync());
                if (res != 0) {
                    if (!(await JSRuntime.InvokeAsync<bool>("confirm",
                                                            String.Format(
                                                                format: "An diesem Tag wurde(n) bereits {0} Bestellung(en) vorgefunden.\n" +
                                                                        "Dennoch eintragen? (Ja)",
                                                                arg0: res)))) {
                        foreach (PersonOrderCard x in personOrderCards) {
                            x.ResetMoney();
                        }
                        return;
                    }
                }
            }

            // collect orders
            List<Order> orders = new List<Order>();
            Provider p = providers.Find(x => x.Guid.Equals(selectedProvider))!;
            for (int i = 0; i < person.Count; i += 1) {
                if (money[i].Equals(new Money(0, 0))) {
                    // empty -> so do nothing
                }
                else {
                    orders.Add(new Order(
                        dateTime: selectedDate.AddMilliseconds(orders.Count + res),
                        money: money[i],
                        provider: p,
                        referencedPerson: person[i].Guid
                    ));
                }
            }

            if (orders.Count == 0) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNobodyHasChanges)], ToastLevel.Info);
                foreach (PersonOrderCard x in personOrderCards) {
                    x.ResetMoney();
                }
                return;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "order/insertNewList"),
                Content = new StringContent(JsonConvert.SerializeObject(orders), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Error);
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastOrderSended)], ToastLevel.Success);
                foreach (PersonOrderCard x in personOrderCards) {
                    x.ResetMoney();
                }
            }
        }

        private async Task LoadAndShowHistory(int index) {
            // toggle preview
            showHistory[index] = !showHistory[index];

            // load data
            Guid personGuid = person[index].Guid;

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "order/fromPerson"),
                Headers = {
                    { "guid", personGuid.ToString() },
                    { "amount", Amount.ToString() }
                }
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                shownOrders[index].Clear();
                shownOrders[index] = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? new List<Order>();
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
            }
        }

        private async void HistoryLoadInModal(int index) {
            completeHistoryFromOne.Clear();

            // load data
            Guid personGuid = person[index].Guid;
            ModalTitle = _loc[nameof(RessourceAppStrings.historyOf)] + ": " + person[index].Name;

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "order/fromPerson"),
                Headers = {
                    { "guid", personGuid.ToString() }
                    // empty amount = complete
                }
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                completeHistoryFromOne = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? new List<Order>();
                ModalDialog!.Open();
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
            }
        }

        private async void LoadOrdersForOverview() {
            if (orderListForOverview.Count == 0) {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "order/requestAmount"),
                    Headers = {
                        { "amount", 500.ToString() }
                    }
                };

                HttpResponseMessage? response;
                try {
                    response = await HttpClient.SendAsync(httpRequestMessage);
                }
                catch {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }

                if (response.IsSuccessStatusCode) {
                    List<Order> order = JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync()) ?? new List<Order>();

                    while (order.Count != 0) {
                        List<Order?> dayOrders = new List<Order?>();
                        // initialize day with null
                        for (int i = 0; i < person.Count; i += 1) {
                            dayOrders.Add(null);
                        }
                        // if order at day insert it
                        Console.WriteLine(order[0].DateTime.Date + " - " + referenceDate.Date);
                        while (order[0].DateTime.Date == referenceDate.Date) {
                            // insert the order to person-row
                            dayOrders[person.FindIndex(x => x.Guid.Equals(order[0].ReferencedPerson))] = order[0];
                            order.RemoveAt(0);
                            if (order.Count == 0) {
                                break;
                            }
                        }
                        // add the day-orders
                        orderListForOverview.Add(new DateAndOrder(
                            DateTime: referenceDate,
                            Orders: dayOrders)
                        );
                        // go to the day before
                        referenceDate = referenceDate.AddDays(-1);
                    }
                    base.StateHasChanged();
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
                }
            }
        }
    }
}
