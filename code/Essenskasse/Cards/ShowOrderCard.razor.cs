using Microsoft.AspNetCore.Components;
using Modells;

namespace Essenskasse.Cards {

    public partial class ShowOrderCard {

        [Parameter]
        public Order Order{ get; set; } = new Order();
        [Parameter]
        public string DesignationOfPerson { get; set; } = "";
        [Parameter]
        public int OrderNumberInList { get; set; }
        [Parameter]
        public EventCallback<int> ButtonChangeClickCallback { get; set; }

        private void ClickOnChange() {
            ButtonChangeClickCallback.InvokeAsync(OrderNumberInList);
        }
    }
}
