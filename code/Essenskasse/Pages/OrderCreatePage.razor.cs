using Essenskasse.Cards;
using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    public partial class OrderCreatePage {

        private string dashboardLink = "/";
        private HistoryModalCard? ModalDialog { get; set; }

        private List<Provider> providers = [];
        private List<Person> person = [];
        private List<Order> completeHistoryFromOne = [];

        /// <summary>Eingetippter Betrag je Person (Text, wie getippt).</summary>
        private readonly Dictionary<Guid, string> amountTexts = [];

        /// <summary>Geladene Bestellverlaeufe je Person.</summary>
        private readonly Dictionary<Guid, List<Order>> histories = [];

        /// <summary>Personen, deren Verlauf aufgeklappt ist.</summary>
        private readonly HashSet<Guid> openHistory = [];

        private bool isSubmitting = false;

        private record DateAndOrder(List<string> Head, DateTime DateTime, List<Order?> Orders);
        private List<DateAndOrder> orderListForOverview = [];
        private DateTime referenceDate = DateTime.Now;
        private int overviewOrderAlreadyLoadCount = 0;

        private string ModalTitle = "";

        private DateTime selectedDate = DateTime.Today;

        private enum OrderView { Order, Overview }
        private OrderView view = OrderView.Order;

        /// <summary>Das Datum fuer das Datumsfeld; ungueltige oder geleerte Eingaben werden ignoriert.</summary>
        private DateTime SelectedDateValue {
            get => selectedDate;
            set {
                if (value.Year >= 2000) {
                    selectedDate = value.Date;
                }
            }
        }

        private bool IsDate(int dayOffset) => selectedDate.Date == DateTime.Today.AddDays(dayOffset);

        private void SetDateOffset(int dayOffset) {
            selectedDate = DateTime.Today.AddDays(dayOffset);
        }

        private async Task ShowView(OrderView target) {
            view = target;
            if (target == OrderView.Overview && orderListForOverview.Count == 0 && !noMoreToLoad) {
                await LoadOrdersForOverview();
            }
        }

        private Guid selectedProvider = new Guid();

        private bool noMoreToLoad = false;

        private int OpacityOfNoMoreToLoad {
            get {
                return noMoreToLoad ? 50 : 100;
            }
        }

        private int Amount {
            get => AppData.LoadAmountOrder;
        }

        /// <summary></summary>
        protected override async Task OnInitializedAsync() {
            if (AuthStateService.IsAuthenticated) {
                await RequestAllProvider();
                await RequestAllPerson();
            }
            base.StateHasChanged();
        }

        /// <summary></summary>
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
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.noProviderPleaseInsertBefore)], ToastLevel.Info);
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        /// <summary></summary>
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
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        private List<Order> HistoryOf(Guid guid) => histories.TryGetValue(guid, out List<Order>? list) ? list : [];

        private string TextOf(Guid guid) => amountTexts.TryGetValue(guid, out string? text) ? text : "";

        private void SetText(Guid guid, string text) {
            amountTexts[guid] = text;
        }

        /// <summary>Liest den Betrag einer Person. Leer gilt als 0 (keine Bestellung); gueltig sind 0 und positive Betraege.</summary>
        private bool TryAmountCents(Guid guid, out long cents) {
            cents = 0;
            string text = TextOf(guid).Trim();
            if (text.Length == 0) {
                return true;
            }
            if (MoneyParser.TryParse(text, out Money parsed) && parsed.ToTotalCents() >= 0) {
                cents = parsed.ToTotalCents();
                return true;
            }
            return false;
        }

        private bool IsInvalid(Guid guid) => !TryAmountCents(guid, out _);

        private bool IsInconspicuous(Person x) => x.Saldo.IsGreater(AppData.DashboardMoneyLimit);

        private static string InputIdOf(Person x) => "ek-ord-" + x.Guid.ToString("N");

        /// <summary>Springt mit Enter zum Betragsfeld der naechsten Person.</summary>
        private async Task FocusNext(Guid guid) {
            int index = person.FindIndex(x => x.Guid.Equals(guid));
            if (index >= 0 && index + 1 < person.Count) {
                await JSRuntime.InvokeVoidAsync("ekFocus", InputIdOf(person[index + 1]));
            }
        }

        private void ClearAllAmounts() {
            amountTexts.Clear();
        }

        /// <summary></summary>
        private async Task PostOrderForTheDay() {
            if (isSubmitting) {
                return;
            }
            if (person.Any(x => IsInvalid(x.Guid))) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastMoneyFormatInvalid)], ToastLevel.Warning);
                return;
            }
            isSubmitting = true;
            try {
                await PostOrderForTheDayCore();
            }
            finally {
                isSubmitting = false;
            }
        }

        /// <summary></summary>
        private async Task PostOrderForTheDayCore() {
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
                        return;
                    }
                }
            }

            // collect orders
            List<Order> orders = new List<Order>();
            Provider p = providers.Find(x => x.Guid.Equals(selectedProvider))!;
            foreach (Person x in person) {
                TryAmountCents(x.Guid, out long cents);
                if (cents > 0) {
                    orders.Add(new Order(
                        dateTime: selectedDate.Date.AddMilliseconds(orders.Count + res),
                        money: Money.FromTotalCents(cents),
                        provider: p,
                        referencedPerson: x.Guid
                    ));
                }
            }

            if (orders.Count == 0) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNobodyHasChanges)], ToastLevel.Info);
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
                ClearAllAmounts();
            }
        }

        /// <summary>Klappt den Verlauf einer Person auf oder zu und laedt ihn beim Aufklappen.</summary>
        private async Task ToggleHistory(Person x) {
            if (!openHistory.Add(x.Guid)) {
                openHistory.Remove(x.Guid);
                return;
            }
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "order/fromPerson"),
                    Headers = {
                        { "guid", x.Guid.ToString() },
                        { "amount", Amount.ToString() }
                    }
                });
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    histories[x.Guid] = json.Length == 0 ? [] : JsonConvert.DeserializeObject<List<Order>>(json) ?? [];
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
                }
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
        }

        /// <summary>Laedt die komplette Historie einer Person in den Dialog.</summary>
        private async Task HistoryLoadInModal(Person x) {
            completeHistoryFromOne.Clear();
            ModalTitle = _loc[nameof(RessourceAppStrings.historyOf)] + ": " + x.Name;
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "order/fromPerson"),
                    Headers = {
                        { "guid", x.Guid.ToString() }
                        // empty amount = complete
                    }
                });
                if (response.IsSuccessStatusCode) {
                    string json = await response.Content.ReadAsStringAsync();
                    completeHistoryFromOne = json.Length == 0 ? [] : JsonConvert.DeserializeObject<List<Order>>(json) ?? [];
                    ModalDialog!.Open();
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
                }
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
        }

        /// <summary></summary>
        private async Task LoadOrdersForOverview() {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "order/requestNextAmount"),
                Headers = {
                    { "amount", 100.ToString() },
                    { "alreadyLoaded", overviewOrderAlreadyLoadCount.ToString() }
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
                overviewOrderAlreadyLoadCount += order.Count;
                referenceDate = referenceDate.AddDays(1); // for the next load essential

                if (order.Count == 0) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.nothingMoreToLoad)], ToastLevel.Info);
                    noMoreToLoad = true;
                    base.StateHasChanged();
                    return;
                }
                while (order.Count != 0) {
                    List<Order?> dayOrders = new List<Order?>();
                    if (!orderListForOverview.Exists(x => x.DateTime.Date == referenceDate.Date)) {
                        List<string> head = [_loc[nameof(RessourceAppStrings.date)]];
                        // initialize day with null && the head
                        for (int i = 0; i < person.Count; i += 1) {
                            dayOrders.Add(null);
                            head.Add(person[i].Name);
                        }
                        // if order at day insert it
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
                            Head: head,
                            DateTime: referenceDate,
                            Orders: dayOrders)
                        );
                    }
                    else { // day already exist -> update cells at last day, other should be irrelevant
                        while (order[0].DateTime.Date == referenceDate.Date) {
                            int i = person.FindIndex(x => x.Guid.Equals(order[0].ReferencedPerson));
                            orderListForOverview[^1].Orders[i] = order[0];
                            order.RemoveAt(0);
                            if (order.Count == 0) {
                                break;
                            }
                        }
                    }
                    // go to the day before
                    referenceDate = referenceDate.AddDays(-1);
                }
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }
        }

        /// <summary>
        /// Check which content should be printed in the cell.
        /// </summary>
        /// <param name="x">Order, that contain the Money-Value or is null if not</param>
        /// <returns>if Money-Value is null => "-" and otherwise the Money.ToString()</returns>
        private static string OverviewCellFillContent(Order? x) => (x is null) ? "-" : x!.Money.ToString();

        private static string OverviewCellFill(DateAndOrder x) {
            bool isWeekend = x.DateTime.DayOfWeek == DayOfWeek.Saturday || x.DateTime.DayOfWeek == DayOfWeek.Sunday;
            bool hasOrder = x.Orders != null;

            if (isWeekend) {
                if (hasOrder) {
                    return "background-color: #FFDDC1;";
                }
                else {
                    return "background-color: #FFE4E1;";
                }
            }
            else {
                if (hasOrder) {
                    return "background-color: #DFF0D8;";
                }
                else {
                    return "background-color: #F9F9F9;";
                }
            }
        }
    }
}
