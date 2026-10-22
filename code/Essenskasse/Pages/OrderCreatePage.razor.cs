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
        private bool historyDialogOpen = false;
        private readonly HashSet<Guid> loadingHistory = [];

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

        /// <summary>Zuletzt bestellter Betrag je Person (Vorschlag "wie zuletzt").</summary>
        private Dictionary<Guid, Money> lastAmounts = [];

        private string personFilter = "";
        private bool onlyFilled = false;

        /// <summary>Anzahl der Personen mit eingetragenem Betrag (auch wenn sie gerade ausgeblendet sind).</summary>
        private int FilledCount => person.Count(x => TextOf(x.Guid).Trim().Length > 0);

        /// <summary>Personen passend zu Suche und Filter (die Eingaben ausgeblendeter Personen bleiben erhalten).</summary>
        private IEnumerable<Person> ShownPersons {
            get {
                string text = personFilter.Trim();
                IEnumerable<Person> result = person;
                if (text.Length > 0) {
                    result = result.Where(x => x.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase));
                }
                if (onlyFilled) {
                    result = result.Where(x => TextOf(x.Guid).Trim().Length > 0);
                }
                return result;
            }
        }

        private void ClearPersonFilter() {
            personFilter = "";
            onlyFilled = false;
        }

        private const int OverviewPageSize = 100;

        /// <summary>Alle bisher fuer die Uebersicht geladenen Bestellungen (neueste zuerst).</summary>
        private readonly List<Order> overviewOrders = [];
        private List<OrderOverviewDay> overviewDays = [];
        private List<Person> overviewPeople = [];
        private bool overviewLoading = false;
        private bool noMoreToLoad = false;

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

        /// <summary>Montag bis Sonntag der aktuellen Woche (es wird woechentlich abgerechnet).</summary>
        private IReadOnlyList<DateTime> ThisWeek => WeekDays.OfWeek(DateTime.Today);

        private void SetDate(DateTime day) {
            selectedDate = day.Date;
        }

        private async Task ShowView(OrderView target) {
            view = target;
            if (target == OrderView.Overview && overviewOrders.Count == 0 && !noMoreToLoad) {
                await LoadOrdersForOverview();
            }
        }

        private Guid selectedProvider = new Guid();

        private int Amount {
            get => AppData.LoadAmountOrder;
        }

        /// <summary></summary>
        protected override async Task OnInitializedAsync() {
            if (AuthStateService.IsAuthenticated) {
                await RequestAllProvider();
                await RequestAllPerson();
                await LoadLastAmounts();
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

        /// <summary>Laedt je Person den Betrag der letzten Bestellung; schlaegt das fehl, fehlt nur der Vorschlag.</summary>
        private async Task LoadLastAmounts() {
            try {
                HttpResponseMessage response = await HttpClient.GetAsync(Config["API-Adress"] + "order/lastAmounts");
                if (response.IsSuccessStatusCode) {
                    List<LastOrderAmount> list = JsonConvert.DeserializeObject<List<LastOrderAmount>>(await response.Content.ReadAsStringAsync()) ?? [];
                    lastAmounts = list.Where(x => x.Money.ToTotalCents() > 0).ToDictionary(x => x.ReferencedPerson, x => x.Money);
                }
            }
            catch {
                lastAmounts = [];
            }
        }

        private Money? LastOf(Guid guid) => lastAmounts.TryGetValue(guid, out Money? money) ? money : null;

        /// <summary>Addiert einen Schnellbetrag zum eingetippten Betrag (ungueltige Eingaben werden dabei ersetzt).</summary>
        private void AddQuick(Guid guid, long cents) {
            TryAmountCents(guid, out long current);
            SetText(guid, QuickAmounts.ToInputText(current + cents));
        }

        private void ApplyLast(Guid guid) {
            if (lastAmounts.TryGetValue(guid, out Money? money)) {
                SetText(guid, QuickAmounts.ToInputText(money.ToTotalCents()));
            }
        }

        private void CloseHistoryDialog() {
            historyDialogOpen = false;
        }

        /// <summary>Die Bestellungen des Dialogs, nach Monat gruppiert (neueste zuerst).</summary>
        private IEnumerable<IGrouping<DateTime, Order>> CompleteHistoryByMonth =>
            completeHistoryFromOne.OrderByDescending(x => x.DateTime).GroupBy(x => new DateTime(x.DateTime.Year, x.DateTime.Month, 1));

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
            List<Person> shown = ShownPersons.ToList();
            int index = shown.FindIndex(x => x.Guid.Equals(guid));
            if (index >= 0 && index + 1 < shown.Count) {
                await JSRuntime.InvokeVoidAsync("ekFocus", InputIdOf(shown[index + 1]));
            }
        }

        private void ClearAllAmounts() {
            amountTexts.Clear();
        }

        private bool previewOpen = false;
        private int existingOrdersOnDay = 0;

        /// <summary>Alle Personen mit eingetragenem Betrag groesser 0 samt Betrag in Cent.</summary>
        private List<(Person Person, long Cents)> Entries {
            get {
                List<(Person, long)> result = [];
                foreach (Person x in person) {
                    TryAmountCents(x.Guid, out long cents);
                    if (cents > 0) {
                        result.Add((x, cents));
                    }
                }
                return result;
            }
        }

        private long TotalCents => Entries.Sum(x => x.Cents);

        private Provider? SelectedProviderData => providers.Find(x => x.Guid.Equals(selectedProvider));

        private void ClosePreview() {
            previewOpen = false;
        }

        /// <summary>Prueft die Eingaben, fragt die vorhandenen Bestellungen des Tages ab und oeffnet die Bestaetigung.</summary>
        private async Task OpenPreview() {
            if (isSubmitting) {
                return;
            }
            if (SelectedProviderData is null) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.orderNoProvider)], ToastLevel.Info);
                return;
            }
            if (person.Any(x => IsInvalid(x.Guid))) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastMoneyFormatInvalid)], ToastLevel.Warning);
                return;
            }
            if (Entries.Count == 0) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNobodyHasChanges)], ToastLevel.Info);
                return;
            }
            isSubmitting = true;
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "order/requestDate"),
                    Headers = { { "dateString", selectedDate.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffK") } }
                });
                if (!response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }
                existingOrdersOnDay = int.Parse(await response.Content.ReadAsStringAsync());
                previewOpen = true;
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
            finally {
                isSubmitting = false;
            }
        }

        /// <summary>Sendet die Bestellungen der Bestaetigung ab (alles oder nichts, das erledigt die API).</summary>
        private async Task ConfirmOrder() {
            if (isSubmitting) {
                return;
            }
            Provider? provider = SelectedProviderData;
            List<(Person Person, long Cents)> entries = Entries;
            if (provider is null || entries.Count == 0) {
                previewOpen = false;
                return;
            }
            isSubmitting = true;
            try {
                List<Order> orders = [];
                foreach ((Person x, long cents) in entries) {
                    orders.Add(new Order(
                        dateTime: selectedDate.Date.AddMilliseconds(orders.Count + existingOrdersOnDay),
                        money: Money.FromTotalCents(cents),
                        provider: provider,
                        referencedPerson: x.Guid));
                }
                HttpResponseMessage response = await HttpClient.SendAsync(new HttpRequestMessage() {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Config["API-Adress"] + "order/insertNewList"),
                    Content = new StringContent(JsonConvert.SerializeObject(orders), Encoding.UTF8, "application/json")
                });
                if (response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastOrderSended)], ToastLevel.Success);
                    previewOpen = false;
                    ClearAllAmounts();
                    ResetOverview();
                    await LoadLastAmounts();
                    await RequestAllPerson(); // neue Salden anzeigen
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Error);
                }
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
            finally {
                isSubmitting = false;
            }
        }

        /// <summary>Klappt den Verlauf einer Person auf oder zu und laedt ihn beim Aufklappen.</summary>
        private async Task ToggleHistory(Person x) {
            if (!openHistory.Add(x.Guid)) {
                openHistory.Remove(x.Guid);
                return;
            }
            loadingHistory.Add(x.Guid);
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
            finally {
                loadingHistory.Remove(x.Guid);
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
                    historyDialogOpen = true;
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
                }
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
        }

        /// <summary>Verwirft die geladene Uebersicht (z. B. nach neuen Bestellungen), sie wird beim naechsten Oeffnen neu geladen.</summary>
        private void ResetOverview() {
            overviewOrders.Clear();
            overviewDays = [];
            overviewPeople = [];
            noMoreToLoad = false;
        }

        /// <summary>Laedt die naechsten Bestellungen und baut die Tage der Uebersicht neu auf.</summary>
        private async Task LoadOrdersForOverview() {
            if (overviewLoading) {
                return;
            }
            overviewLoading = true;
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "order/requestNextAmount"),
                    Headers = {
                        { "amount", OverviewPageSize.ToString() },
                        { "alreadyLoaded", overviewOrders.Count.ToString() }
                    }
                });
                if (!response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
                    return;
                }
                string json = await response.Content.ReadAsStringAsync();
                List<Order> loaded = json.Length == 0 ? [] : JsonConvert.DeserializeObject<List<Order>>(json) ?? [];
                if (loaded.Count == 0) {
                    noMoreToLoad = true;
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.nothingMoreToLoad)], ToastLevel.Info);
                }
                else {
                    overviewOrders.AddRange(loaded);
                    noMoreToLoad = loaded.Count < OverviewPageSize;
                    overviewDays = OrderOverview.Build(overviewOrders);
                    HashSet<Guid> ordered = OrderOverview.PeopleWithOrders(overviewDays);
                    overviewPeople = person.Where(x => ordered.Contains(x.Guid)).ToList();
                }
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
            finally {
                overviewLoading = false;
            }
        }

        private static long CentsOf(OrderOverviewDay day, Person x) => day.CentsByPerson.TryGetValue(x.Guid, out long cents) ? cents : 0;

        private static bool IsWeekend(DateTime date) => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}
