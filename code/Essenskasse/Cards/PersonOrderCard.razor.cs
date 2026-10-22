using Microsoft.AspNetCore.Components;
using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;

namespace Essenskasse.Cards {

    public partial class PersonOrderCard {

        [Parameter]
        public string PersonName { get; set; } = "";

        [Parameter]
        public Money Money { get; set; } = new Money(0, 0);

        [Parameter]
        public EventCallback<Money> OnValuesChanged { get; set; }

        private decimal _orderAmount = 0;
        private decimal orderAmount {
            get {
                return _orderAmount;
            }
            set {
                if (value > 0) {
                    long euro = (long)value;
                    short cent = (short)((value - euro) * 100);
                    if (cent < 100) {
                        _orderAmount = value;
                    }
                    else {
                        // nothing
                    }
                }
                else {
                    _orderAmount = 0;
                }
            }
        }

        private async Task UpdateValues() {
            // inform the parent that a change is there
            await OnValuesChanged.InvokeAsync(Money);
        }

        private async void orderToMoney() {
            try {
                long euro = (long) _orderAmount;
                short cent = (short)((_orderAmount - euro) * 100);
                Money = new Money(euro, cent);
                await UpdateValues();
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastMoneyFormatInvalid)], ToastLevel.Error);
            }
        }

        public async void ResetMoney () {
            Money = new Money(0, 0);
            orderAmount = 0;
            await UpdateValues();
        }
    }
}
