using Essenskasse.Client;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;

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
