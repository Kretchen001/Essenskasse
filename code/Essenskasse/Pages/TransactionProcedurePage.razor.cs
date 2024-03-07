using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    public partial class TransactionProcedurePage {

        private List<TransactionProcedure> transactions = [];

        /// <summary>internalValue</summary>
        private decimal _amount = 0;
        private decimal amount {
            get {
                return _amount;
            }
            set {
                if (value != 0) {
                    long euro = (long)value;
                    short cent = (short)((value - euro) * 100);
                    if (cent < 100) {
                        _amount = value;
                        amountMoney = new Money(euro, cent);
                    }
                    else {
                        // nothing
                    }
                }
            }
        }

        /// <summary></summary>
        private Money amountMoney = new Money();
        
        /// <summary></summary>
        private string comment = "";

        /// <summary></summary>
        private Money currentCashBalance = new Money(0, 0);

        protected override async Task OnInitializedAsync() {
            await requestAllTransactions();
            currentCashBalanceCalculate();
            base.StateHasChanged();
        }

        private async Task requestAllTransactions() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "transactionprocedure/all")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                transactions.Clear();
                transactions = JsonConvert.DeserializeObject<List<TransactionProcedure>>(await response.Content.ReadAsStringAsync()) ?? new List<TransactionProcedure>();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
            }
        }

        private async Task insertNewTransaction() {
            // Request all Person, that are saved in the database
            TransactionProcedure temp = new TransactionProcedure() {
                DateTime = DateTime.Now,
                Amount = amountMoney,
                Comment = comment
            };
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "transactionprocedure/new"),
                Content = new StringContent(JsonConvert.SerializeObject(temp), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                ToastService.ShowToast("Erfolgreich übermittelt.", ToastLevel.Success);
                transactions.Add(temp);
                currentCashBalanceCalculate();
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
            }
        }

        private void currentCashBalanceCalculate() {
            currentCashBalance = new Money(0, 0);
            foreach (TransactionProcedure x in transactions) {
                if (x.Amount.IsPositiv()) {
                    currentCashBalance = new Money().CalculateMoney(CalculationOperation.Addition, currentCashBalance, x.Amount) ?? new Money(0, 0);
                }
                else {
                    Money temp = new Money() {
                        Euro = Math.Abs(x.Amount.Euro),
                        Cent = Math.Abs(x.Amount.Cent),
                    };
                    currentCashBalance = new Money().CalculateMoney(CalculationOperation.Substraction, currentCashBalance, temp) ?? new Money(0, 0);
                }
            }
        }
    }
}
