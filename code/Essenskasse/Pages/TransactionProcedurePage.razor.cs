using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    public partial class TransactionProcedurePage {

        private List<TransactionProcedure> transactions = [];

        /// <summary>internalValue</summary>
        private decimal _Amount = 0;
        private decimal Amount {
            get {
                return _Amount;
            }
            set {
                if (value != 0) {
                    long euro = (long)value;
                    short cent = (short)((value - euro) * 100);
                    if (cent < 100) {
                        _Amount = value;
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
            await RequestAllTransactions();
            CurrentCashBalanceCalculate();
            base.StateHasChanged();
        }

        private async Task RequestAllTransactions() {
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

        private async Task InsertNewTransaction() {
            if (amountMoney.Equals(new Money(0, 0))) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastMoneyFormatInvalid)], ToastLevel.Warning);
                return;
            }
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
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.successfullyTransmitted)], ToastLevel.Success);
                transactions.Add(temp);
                CurrentCashBalanceCalculate();
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
            }
        }

        private void CurrentCashBalanceCalculate() {
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
