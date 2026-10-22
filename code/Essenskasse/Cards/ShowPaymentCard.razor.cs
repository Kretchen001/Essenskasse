using Microsoft.AspNetCore.Components;
using Modells;

namespace Essenskasse.Cards {

    public partial class ShowPaymentCard {

        [Parameter]
        public Payment Payment { get; set; } = new Payment();
        [Parameter]
        public string DesignationOfPerson { get; set; } = "";
        [Parameter]
        public int PaymentNumberInList { get; set; }
        [Parameter]
        public EventCallback<int> ButtonChangeClickCallback { get; set; }

        public string GetCommentUnescape() {
            return Uri.UnescapeDataString(Payment.Comment);
        }

        private void ClickOnChange() {
            ButtonChangeClickCallback.InvokeAsync(PaymentNumberInList);
        }
    }
}
