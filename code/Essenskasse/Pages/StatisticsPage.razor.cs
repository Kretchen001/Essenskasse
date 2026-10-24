using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;
using Modells.Statistics;
using Newtonsoft.Json;

namespace Essenskasse.Pages {

    /// <summary>Auswertungen: Verlaeufe und Haeufigkeiten aus allen Einzahlungen und Bestellungen.</summary>
    public partial class StatisticsPage {

        private string dashboardLink = "/";
        private bool loading = true;
        private StatisticsData? data;
        private DateTime fromDate = DateTime.Today.AddMonths(-12);
        private DateTime toDate = DateTime.Today;

        protected override async Task OnInitializedAsync() {
            if (!AuthStateService.IsAuthenticated) {
                loading = false;
                return;
            }
            List<Person>? persons = await FetchAsync<Person>("person/requestAll");
            List<Payment>? payments = await FetchAsync<Payment>("payment/requestAll");
            List<Order>? orders = await FetchAsync<Order>("order/requestAll");
            if (persons is not null && payments is not null && orders is not null) {
                persons.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                orders.ForEach(x => x.Provider.Designation = Uri.UnescapeDataString(x.Provider.Designation ?? ""));
                data = new StatisticsData(persons, payments, orders);
                // Beginnt die Kasse erst vor kurzem, startet die Ansicht mit dem ersten Tag der Daten.
                if (data.Span is { } span && span.First > fromDate) {
                    fromDate = span.First;
                }
            }
            loading = false;
        }

        private void SetMonths(int months) {
            toDate = DateTime.Today;
            fromDate = DateTime.Today.AddMonths(-months);
        }

        private void SetAll() {
            toDate = DateTime.Today;
            fromDate = data?.Span?.First ?? DateTime.Today.AddMonths(-12);
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
    }
}
