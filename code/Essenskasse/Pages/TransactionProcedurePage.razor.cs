using Essenskasse.Client;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Components;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    /// <summary>Kaffeekasse: Anzeige fuer alle, Eintragen nur angemeldet.</summary>
    public partial class TransactionProcedurePage {

        private const int PageSize = 25;

        [Inject] private HttpClient HttpClient { get; set; } = default!;
        [Inject] private IConfiguration Config { get; set; } = default!;
        [Inject] private ToastService ToastService { get; set; } = default!;

        private List<TransactionProcedure> transactions = [];
        private CoffeeFundSummary.Result summary = CoffeeFundSummary.Summarize([]);
        private int visibleCount = PageSize;
        private bool loading = true;
        private bool loadFailed = false;
        private bool isBooking = false;
        private bool previewOpen = false;

        /// <summary>false = Einzahlung in die Kasse, true = Auszahlung aus der Kasse.</summary>
        private bool isPayout = false;
        private string amountText = "";
        private string comment = "";

        private bool AmountEntered => amountText.Trim().Length > 0;

        /// <summary>Gueltig ist ein Betrag groesser als 0; die Richtung bestimmt der Umschalter, kein Minuszeichen.</summary>
        private bool AmountValid => TryGetAmountCents(out _);

        /// <summary>Der Kommentar ist ein Pflichtfeld.</summary>
        private bool CommentValid => comment.Trim().Length > 0;

        private bool CanBook => AmountValid && CommentValid;

        /// <summary>Der Betrag mit Vorzeichen: Einzahlung positiv, Auszahlung negativ.</summary>
        private Money SignedAmount {
            get {
                TryGetAmountCents(out long cents);
                return Money.FromTotalCents(isPayout ? -cents : cents);
            }
        }

        private void OpenPreview() {
            previewOpen = CanBook;
        }

        private void ClosePreview() {
            previewOpen = false;
        }

        /// <summary>Der Kassenstand nach der eingegebenen Buchung.</summary>
        private Money BalanceAfter {
            get {
                TryGetAmountCents(out long cents);
                long delta = isPayout ? -cents : cents;
                return Money.FromTotalCents(summary.Balance.ToTotalCents() + delta);
            }
        }

        private IEnumerable<IGrouping<DateTime, TransactionProcedure>> ShownByMonth =>
            transactions.Take(visibleCount).GroupBy(x => new DateTime(x.DateTime.Year, x.DateTime.Month, 1));

        protected override async Task OnInitializedAsync() {
            await ReloadAsync();
        }

        private bool TryGetAmountCents(out long cents) {
            cents = 0;
            if (MoneyParser.TryParse(amountText, out Money parsed) && parsed.ToTotalCents() > 0) {
                cents = parsed.ToTotalCents();
                return true;
            }
            return false;
        }

        private void SetDirection(bool payout) {
            isPayout = payout;
            previewOpen = false;
        }

        private void ShowMore() {
            visibleCount += PageSize;
        }

        private async Task ReloadAsync() {
            loading = true;
            loadFailed = false;
            try {
                HttpResponseMessage response = await HttpClient.GetAsync(Config["API-Adress"] + "transactionprocedure/all");
                if (response.IsSuccessStatusCode) {
                    transactions = (JsonConvert.DeserializeObject<List<TransactionProcedure>>(await response.Content.ReadAsStringAsync()) ?? [])
                        .OrderByDescending(x => x.DateTime).ToList();
                    summary = CoffeeFundSummary.Summarize(transactions);
                }
                else {
                    loadFailed = true;
                }
            }
            catch {
                loadFailed = true;
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
            loading = false;
        }

        private async Task InsertNewTransaction() {
            if (isBooking || !CanBook || !TryGetAmountCents(out long cents)) {
                return;
            }
            isBooking = true;
            TransactionProcedure entry = new() {
                DateTime = DateTime.Now,
                Amount = Money.FromTotalCents(isPayout ? -cents : cents),
                Comment = comment.Trim()
            };
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(new HttpRequestMessage() {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Config["API-Adress"] + "transactionprocedure/new"),
                    Content = new StringContent(JsonConvert.SerializeObject(entry), Encoding.UTF8, "application/json")
                });
                if (response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.successfullyTransmitted)], ToastLevel.Success);
                    transactions.Insert(0, entry);
                    summary = CoffeeFundSummary.Summarize(transactions);
                    amountText = "";
                    comment = "";
                    previewOpen = false;
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
                }
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
            isBooking = false;
        }
    }
}
