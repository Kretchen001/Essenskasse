using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Pages {

    public partial class Dashboard {

        private bool IsSingleListWish {
            get {
                return AppData.IsSingleListWishDashboard;
            }
            set {
                AppData.IsSingleListWishDashboard = value;
                WriteWishListToLocalStorage(value);
            }
        }

        private bool NoticeableOnRightSide {
            get {
                return AppData.NoticeableOnRightSide;
            }
            set {
                AppData.NoticeableOnRightSide = value;
                WriteNoticableSideToLocalStorage(value);
            }
        }

        private string ReturnListWish {
            get {
                if (IsSingleListWish) {
                    return _loc[nameof(RessourceAppStrings.yes)];
                }
                else {
                    return _loc[nameof(RessourceAppStrings.no)];
                }
            }
        }

        private List<Person> person = new List<Person>();
        private List<int> positivSaldo = new List<int>();

        /// <summary>Personen, deren Saldo unter dem Dashboard-Limit liegt.</summary>
        private List<Person> NoticeablePersons => person.Where((x, i) => !positivSaldo.Contains(i)).ToList();

        /// <summary>Personen, deren Saldo ueber dem Dashboard-Limit liegt.</summary>
        private List<Person> InconspicuousPersons => person.Where((x, i) => positivSaldo.Contains(i)).ToList();

        private Money currentCashBalance = new Money(0, 0);

        protected override async Task OnInitializedAsync() {
            await RequestAllPerson();
            CurrentCashBalanceCalculate();
            base.StateHasChanged();
        }

        private async Task RequestAllPerson() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/requestAll")
            };

            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

            if (response.IsSuccessStatusCode) {
                person.Clear();
                positivSaldo.Clear(); // sonst bleiben die Indizes nach "Saldo neu berechnen" veraltet bzw. doppelt erhalten
                person = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                person.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                person.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                person = person.OrderBy(x => x.Name).ToList();
                for (int i = 0; i < person.Count; i += 1) {
                    if (person[i].Saldo.IsGreater(AppData.DashboardMoneyLimit)) {
                        positivSaldo.Add(i);
                    }
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)] + "\n" + _loc[nameof(RessourceAppStrings.toastNothingRequested)], ToastLevel.Warning);
            }
        }

        private async void RecalculateSaldos() {
            // Request all providers, that are saved in the database.
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "person/updateDatabasePersonSaldo")
            };

            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

            if (response.IsSuccessStatusCode) {
                await RequestAllPerson();
                CurrentCashBalanceCalculate();
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async void WriteWishListToLocalStorage(bool value) {
            await LocalStorage.SetItemAsync<bool>("IsSingleWishListDashboard", value);
        }

        private void CurrentCashBalanceCalculate() {
            currentCashBalance = new Money(0, 0);
            foreach (Person x in person) {
                if (x.Saldo.IsPositiv()) {
                    currentCashBalance = new Money().CalculateMoney(CalculationOperation.Addition, currentCashBalance, x.Saldo) ?? new Money(0, 0);
                }
                else {
                    Money temp = new Money() {
                        Euro = Math.Abs(x.Saldo.Euro),
                        Cent = Math.Abs(x.Saldo.Cent),
                    };
                    currentCashBalance = new Money().CalculateMoney(CalculationOperation.Substraction, currentCashBalance, temp) ?? new Money(0, 0);
                }
            }
        }

        private void ToggleColumnInView() {
            NoticeableOnRightSide = !NoticeableOnRightSide;
        }

        private async void WriteNoticableSideToLocalStorage(bool value) {
            await LocalStorage.SetItemAsync<bool>("NoticeableOnRightSide", value);
        }
    }
}
