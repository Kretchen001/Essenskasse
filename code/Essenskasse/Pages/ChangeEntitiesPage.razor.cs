using Essenskasse.Client;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    /// <summary>Aenderungen: bestehende Einzahlungen und Bestellungen ansehen (und korrigieren).</summary>
    public partial class ChangeEntitiesPage {

        private string dashboardLink = "/";
        private bool loading = true;
        private bool loadingMore = false;
        private bool showOrders = false;
        private bool noMorePayments = false;
        private bool noMoreOrders = false;

        private List<Person> PersonList { get; set; } = [];
        private List<Provider> Providers { get; set; } = [];
        private List<Payment> Payments { get; set; } = [];
        private List<Order> Orders { get; set; } = [];

        /// <summary>Die Einzahlungen, nach Monat gruppiert (die Liste ist bereits neueste zuerst).</summary>
        private IEnumerable<IGrouping<DateTime, Payment>> PaymentsByMonth =>
            Payments.GroupBy(x => new DateTime(x.DateTime.Year, x.DateTime.Month, 1));

        private IEnumerable<IGrouping<DateTime, Order>> OrdersByMonth =>
            Orders.GroupBy(x => new DateTime(x.DateTime.Year, x.DateTime.Month, 1));

        protected override async Task OnInitializedAsync() {
            if (!AuthStateService.IsAuthenticated) {
                loading = false;
                return;
            }
            await Task.WhenAll(RequestPerson(), RequestProvider(), RequestPayments(), RequestOrders());
            loading = false;
        }

        private void ShowOrders(bool orders) {
            showOrders = orders;
        }

        // ---------- Bearbeiten ----------

        private Payment? editPayment;
        private Order? editOrder;
        private string amountText = "";
        private string commentText = "";
        private Guid personChoice = Guid.Empty;
        private Guid providerChoice = Guid.Empty;
        private bool isSaving = false;
        private bool confirmingReverse = false;

        private bool EditOpen => editPayment is not null || editOrder is not null;

        private string EditTitle => _loc[editPayment is not null
            ? nameof(RessourceAppStrings.changeEditPayment)
            : nameof(RessourceAppStrings.changeEditOrder)].Value;

        private DateTime EditDate => editPayment?.DateTime ?? editOrder?.DateTime ?? DateTime.MinValue;

        /// <summary>Der bisherige Betrag der geoeffneten Buchung.</summary>
        private string EditOriginalAmount => (editPayment?.Amount ?? editOrder?.Money)?.ToString() ?? "";

        /// <summary>Ohne Aenderungstoken kann der Server nichts aendern.</summary>
        private bool TokenMissing => AppData.AccessToken is null || AppData.AccessToken == Guid.Empty;

        private bool AmountEntered => amountText.Trim().Length > 0;

        /// <summary>Gueltig ist ein Betrag ungleich 0; bei Bestellungen zusaetzlich nur positiv.</summary>
        private bool TryGetAmount(out Money amount) {
            if (!MoneyParser.TryParse(amountText, out amount)) {
                return false;
            }
            long cents = amount.ToTotalCents();
            return cents != 0 && (editPayment is not null || cents > 0);
        }

        private bool AmountValid => TryGetAmount(out _);

        private bool PersonChanged => EditOpen && personChoice != (editPayment?.ReferencedPerson ?? editOrder?.ReferencedPerson);

        private bool AmountChanged =>
            TryGetAmount(out Money amount) && amount.ToTotalCents() != (editPayment?.Amount ?? editOrder?.Money)?.ToTotalCents();

        private bool CommentChanged =>
            editPayment is not null && commentText.Trim() != (editPayment.Comment ?? "").Trim();

        private bool ProviderChanged => editOrder is not null && providerChoice != editOrder.Provider.Guid;

        /// <summary>Wurde gegenueber der Buchung etwas geaendert (bei gueltigem Betrag)?</summary>
        private bool Changed {
            get {
                if (!TryGetAmount(out Money amount)) {
                    return false;
                }
                if (editPayment is not null) {
                    return EntityChanges.PaymentChanged(editPayment, amount, commentText, personChoice);
                }
                return editOrder is not null && EntityChanges.OrderChanged(editOrder, amount, providerChoice, personChoice);
            }
        }

        private bool CanSave => !isSaving && !TokenMissing && AmountValid && Changed
            && (editPayment is not null || Providers.Exists(x => x.Guid == providerChoice));

        /// <summary>CSS-Klassen einer Zeile: stornierte Buchungen werden durchgestrichen, Stornobuchungen abgesetzt.</summary>
        private static string RowClass(string baseClass, long? reversedBy, long? reversalOf) {
            if (reversedBy is not null) {
                return (baseClass + " is-reversed").Trim();
            }
            return reversalOf is not null ? (baseClass + " is-reversal").Trim() : baseClass;
        }

        private void OpenEditPayment(Payment payment) {
            confirmingReverse = false;
            editOrder = null;
            editPayment = payment;
            amountText = EntityChanges.ToInputText(payment.Amount);
            commentText = payment.Comment ?? "";
            personChoice = payment.ReferencedPerson;
            isSaving = false;
        }

        private void OpenEditOrder(Order order) {
            confirmingReverse = false;
            editPayment = null;
            editOrder = order;
            amountText = EntityChanges.ToInputText(order.Money);
            personChoice = order.ReferencedPerson;
            providerChoice = order.Provider.Guid;
            isSaving = false;
        }

        private void CloseEdit() {
            if (isSaving) {
                return;
            }
            confirmingReverse = false;
            editPayment = null;
            editOrder = null;
        }

        private void AskReverse() {
            confirmingReverse = true;
        }

        /// <summary>Storniert die geoeffnete Buchung: Der Server legt eine Gegenbuchung an und korrigiert den Saldo;
        /// die Buchung selbst bleibt sichtbar und wird als storniert markiert.</summary>
        private async Task ReverseAsync() {
            if (isSaving || TokenMissing || (editPayment is null && editOrder is null)) {
                return;
            }
            bool isPayment = editPayment is not null;
            long ticks = isPayment ? editPayment!.DateTime.Ticks : editOrder!.DateTime.Ticks;
            isSaving = true;
            try {
                HttpRequestMessage request = new() {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(Config["API-Adress"] + (isPayment ? "payment/reverse" : "order/reverse"))
                };
                request.Headers.Add("accessToken", AppData.AccessToken!.Value.ToString());
                request.Headers.Add("dateAsLong", ticks.ToString());

                HttpResponseMessage response;
                try {
                    response = await HttpClient.SendAsync(request);
                }
                catch {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }
                if (!response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
                    return;
                }
                string json = await response.Content.ReadAsStringAsync();
                if (isPayment) {
                    Payment reversal = JsonConvert.DeserializeObject<Payment>(json)!;
                    reversal.Comment = Uri.UnescapeDataString(reversal.Comment ?? "");
                    editPayment!.ReversedBy = reversal.DateTime.Ticks;
                    Payments.Add(reversal);
                    Payments = Payments.OrderByDescending(x => x.DateTime).ToList();
                    editPayment = null;
                }
                else {
                    Order reversal = JsonConvert.DeserializeObject<Order>(json)!;
                    editOrder!.ReversedBy = reversal.DateTime.Ticks;
                    Orders.Add(reversal);
                    Orders = Orders.OrderByDescending(x => x.DateTime).ToList();
                    editOrder = null;
                }
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.reverseDone)], ToastLevel.Success);
            }
            finally {
                isSaving = false;
                confirmingReverse = false;
            }
        }

        private async Task SaveAsync() {
            if (!CanSave || !TryGetAmount(out Money amount)) {
                return;
            }
            isSaving = true;
            try {
                if (editPayment is not null) {
                    await SavePaymentAsync(editPayment, amount);
                }
                else if (editOrder is not null) {
                    await SaveOrderAsync(editOrder, amount);
                }
            }
            finally {
                isSaving = false;
            }
        }

        /// <summary>Schickt die Aenderung an die API. Bei Erfolg true; sonst gibt es einen Hinweis und der Dialog bleibt offen.</summary>
        private async Task<bool> SendUpdateAsync(HttpRequestMessage request) {
            HttpResponseMessage response;
            try {
                response = await HttpClient.SendAsync(request);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return false;
            }
            if (!response.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
                return false;
            }
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastUpdateSuccessful)], ToastLevel.Success);
            return true;
        }

        private async Task SavePaymentAsync(Payment original, Money amount) {
            string comment = commentText.Trim();
            // Der Betrag wird immer mitgeschickt; Person und Kommentar nur, wenn sie geaendert wurden.
            HttpRequestMessage request = new() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "payment/update"),
                Content = new StringContent(JsonConvert.SerializeObject(amount), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("accessToken", AppData.AccessToken!.Value.ToString());
            request.Headers.Add("dateAsLong", original.DateTime.Ticks.ToString());
            if (personChoice != original.ReferencedPerson) {
                request.Headers.Add("referencedPerson", personChoice.ToString());
            }
            if (CommentChanged) {
                // Ein leerer Header gilt am Server als "nicht angegeben"; zum Leeren wird deshalb ein (maskiertes) Leerzeichen geschickt.
                request.Headers.Add("comment", comment.Length == 0 ? Uri.EscapeDataString(" ") : Uri.EscapeDataString(comment));
            }

            if (await SendUpdateAsync(request)) {
                int index = Payments.IndexOf(original);
                if (index >= 0) {
                    Payments[index] = new Payment(original.DateTime, amount, personChoice, comment);
                }
                editPayment = null;
            }
        }

        private async Task SaveOrderAsync(Order original, Money amount) {
            Provider? provider = providerChoice == original.Provider.Guid
                ? original.Provider
                : Providers.FirstOrDefault(x => x.Guid == providerChoice);
            if (provider is null) {
                return;
            }
            MoneyProviderGuidData data = new() {
                Money = amount,
                Provider = provider,
                ReferencedPerson = personChoice
            };
            HttpRequestMessage request = new() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "order/update"),
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("accessToken", AppData.AccessToken!.Value.ToString());
            request.Headers.Add("dateAsLong", original.DateTime.Ticks.ToString());

            if (await SendUpdateAsync(request)) {
                int index = Orders.IndexOf(original);
                if (index >= 0) {
                    Orders[index] = new Order(original.DateTime, amount, provider, personChoice);
                }
                editOrder = null;
            }
        }

        private string PersonName(Guid guid) {
            return PersonList.FirstOrDefault(x => x.Guid.Equals(guid))?.Name ?? "";
        }

        /// <summary>Liest eine Liste von der API. Bei einem Fehler gibt es einen Hinweis und null.</summary>
        private async Task<List<T>?> FetchAsync<T>(string path, string failureKey, params (string Name, string Value)[] headers) {
            HttpRequestMessage request = new() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + path)
            };
            foreach ((string name, string value) in headers) {
                request.Headers.Add(name, value);
            }

            HttpResponseMessage response;
            try {
                response = await HttpClient.SendAsync(request);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return null;
            }
            if (!response.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[failureKey], ToastLevel.Warning);
                return null;
            }
            return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync()) ?? [];
        }

        private async Task RequestPerson() {
            List<Person>? loaded = await FetchAsync<Person>("person/requestAll", nameof(RessourceAppStrings.toastNoApiConnection));
            if (loaded is null) {
                return;
            }
            loaded.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
            loaded.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
            PersonList = loaded.OrderBy(x => x.Name).ToList();
        }

        private async Task RequestProvider() {
            List<Provider>? loaded = await FetchAsync<Provider>("provider/requestAll", nameof(RessourceAppStrings.toastNoApiConnection));
            if (loaded is null) {
                return;
            }
            loaded.ForEach(x => x.Designation = Uri.UnescapeDataString(x.Designation));
            Providers = loaded.OrderBy(x => x.Designation).ToList();
        }

        private async Task RequestPayments() {
            List<Payment>? loaded = await FetchAsync<Payment>("payment/requestAmount", nameof(RessourceAppStrings.toastSomethingWrong),
                ("amount", AppData.LoadAmountPayment.ToString()));
            if (loaded is null) {
                return;
            }
            loaded.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment ?? ""));
            Payments = loaded;
            noMorePayments = loaded.Count < AppData.LoadAmountPayment;
        }

        private async Task RequestOrders() {
            List<Order>? loaded = await FetchAsync<Order>("order/requestAmount", nameof(RessourceAppStrings.toastSomethingWrong),
                ("amount", AppData.LoadAmountOrder.ToString()));
            if (loaded is null) {
                return;
            }
            Orders = loaded;
            noMoreOrders = loaded.Count < AppData.LoadAmountOrder;
        }

        private async Task LoadNextAmountOfPayments() {
            loadingMore = true;
            try {
                List<Payment>? loaded = await FetchAsync<Payment>("payment/requestNextAmount", nameof(RessourceAppStrings.toastSomethingWrong),
                    ("amount", AppData.LoadAmountPayment.ToString()),
                    ("alreadyLoaded", Payments.Count.ToString()));
                if (loaded is null) {
                    return;
                }
                loaded.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment ?? ""));
                // Wurde zwischenzeitlich etwas gebucht, verschieben sich die Positionen: bereits gezeigte Eintraege nicht doppelt aufnehmen.
                HashSet<DateTime> known = Payments.Select(x => x.DateTime).ToHashSet();
                Payments.AddRange(loaded.Where(x => !known.Contains(x.DateTime)));
                if (loaded.Count == 0) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNothingToLoad)], ToastLevel.Info);
                    noMorePayments = true;
                }
            }
            finally {
                loadingMore = false;
            }
        }

        private async Task LoadNextAmountOfOrders() {
            loadingMore = true;
            try {
                List<Order>? loaded = await FetchAsync<Order>("order/requestNextAmount", nameof(RessourceAppStrings.toastSomethingWrong),
                    ("amount", AppData.LoadAmountOrder.ToString()),
                    ("alreadyLoaded", Orders.Count.ToString()));
                if (loaded is null) {
                    return;
                }
                HashSet<DateTime> known = Orders.Select(x => x.DateTime).ToHashSet();
                Orders.AddRange(loaded.Where(x => !known.Contains(x.DateTime)));
                if (loaded.Count == 0) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNothingToLoad)], ToastLevel.Info);
                    noMoreOrders = true;
                }
            }
            finally {
                loadingMore = false;
            }
        }
    }
}
