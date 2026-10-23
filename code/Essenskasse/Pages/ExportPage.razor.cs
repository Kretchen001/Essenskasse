using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace Essenskasse.Pages {

    /// <summary>Export: Einzahlungen und Bestellungen eines Zeitraums (optional einer Person) als CSV-Datei.</summary>
    public partial class ExportPage {

        private string dashboardLink = "/";
        private DateTime fromDate = new(DateTime.Today.Year, 1, 1);
        private DateTime toDate = DateTime.Today;
        private string personGuid = "";
        private bool busy = false;
        private List<Person> persons = [];

        protected override async Task OnInitializedAsync() {
            // Die Auswahl der Personen ist nur eine Hilfe; schlaegt das Laden fehl, bleibt "Alle Personen".
            List<Person> loaded = await FetchQuietAsync<Person>("person/requestAll");
            loaded.AddRange(await FetchQuietAsync<Person>("person/requestHidden"));
            loaded.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
            persons = loaded.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private async Task<List<T>> FetchQuietAsync<T>(string path) {
            try {
                HttpResponseMessage response = await HttpClient.GetAsync(Config["API-Adress"] + path);
                if (!response.IsSuccessStatusCode) {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync()) ?? [];
            }
            catch {
                return [];
            }
        }

        /// <summary>Liest eine Liste von der API. Bei einem Fehler gibt es einen Hinweis und null.</summary>
        private async Task<List<T>?> FetchAsync<T>(string path) {
            HttpResponseMessage response;
            try {
                response = await HttpClient.GetAsync(Config["API-Adress"] + path);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return null;
            }
            if (!response.IsSuccessStatusCode) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
                return null;
            }
            return JsonConvert.DeserializeObject<List<T>>(await response.Content.ReadAsStringAsync()) ?? [];
        }

        private async Task ExportAsync() {
            if (fromDate.Date > toDate.Date) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.exportInvalidRange)], ToastLevel.Warning);
                return;
            }
            busy = true;
            try {
                List<Payment>? payments = await FetchAsync<Payment>("payment/requestAll");
                List<Order>? orders = await FetchAsync<Order>("order/requestAll");
                if (payments is null || orders is null) {
                    return;
                }
                if (persons.Count == 0) {
                    await OnInitializedAsync(); // das Laden der Personen war vorhin gescheitert: noch einmal versuchen
                }
                Dictionary<Guid, string> names = persons.GroupBy(x => x.Guid).ToDictionary(g => g.Key, g => g.First().Name);
                string NameOf(Guid guid) => names.TryGetValue(guid, out string? found) ? found : guid.ToString();
                bool allPersons = personGuid.Length == 0;
                Guid chosen = allPersons ? Guid.Empty : Guid.Parse(personGuid);
                bool InRange(DateTime when) => when.Date >= fromDate.Date && when.Date <= toDate.Date;

                List<BookingRow> rows = [];
                string payment = _loc[nameof(RessourceAppStrings.exportKindPayment)];
                string order = _loc[nameof(RessourceAppStrings.exportKindOrder)];
                foreach (Payment entry in payments.Where(x => InRange(x.DateTime) && (allPersons || x.ReferencedPerson == chosen))) {
                    rows.Add(new BookingRow(entry.DateTime, payment, NameOf(entry.ReferencedPerson),
                        Uri.UnescapeDataString(entry.Comment ?? ""), entry.Amount));
                }
                foreach (Order entry in orders.Where(x => InRange(x.DateTime) && (allPersons || x.ReferencedPerson == chosen))) {
                    rows.Add(new BookingRow(entry.DateTime, order, NameOf(entry.ReferencedPerson),
                        Uri.UnescapeDataString(entry.Provider.Designation ?? ""), Money.FromTotalCents(-entry.Money.ToTotalCents())));
                }
                if (rows.Count == 0) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.exportNothing)], ToastLevel.Info);
                    return;
                }
                rows = rows.OrderBy(x => x.When).ToList();

                string[] headers = [
                    _loc[nameof(RessourceAppStrings.exportColDate)], _loc[nameof(RessourceAppStrings.exportColTime)],
                    _loc[nameof(RessourceAppStrings.exportColKind)], _loc[nameof(RessourceAppStrings.exportColPerson)],
                    _loc[nameof(RessourceAppStrings.exportColDetail)], _loc[nameof(RessourceAppStrings.exportColAmount)],
                    _loc[nameof(RessourceAppStrings.exportColCurrency)]
                ];
                string csv = BookingCsv.Build(rows, headers, Money.CurrentCurrency);
                // UTF-8 mit Byte-Order-Mark, damit Excel Umlaute und kyrillische Namen richtig liest.
                UTF8Encoding utf8 = new(true);
                byte[] bytes = [.. utf8.GetPreamble(), .. utf8.GetBytes(csv)];
                string fileName = $"Essenskasse_{fromDate:yyyy-MM-dd}_{toDate:yyyy-MM-dd}.csv";
                await JSRuntime.InvokeVoidAsync("blazorExtensions.DownloadFile", fileName, Convert.ToBase64String(bytes), "text/csv;charset=utf-8");
                ToastService.ShowToast(string.Format(CultureInfo.CurrentCulture, _loc[nameof(RessourceAppStrings.exportDone)].Value, rows.Count), ToastLevel.Success);
            }
            finally {
                busy = false;
            }
        }
    }
}
