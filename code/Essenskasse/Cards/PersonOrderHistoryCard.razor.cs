using Microsoft.AspNetCore.Components;
using Modells;

namespace Essenskasse.Cards {

    public partial class PersonOrderHistoryCard {

        [Parameter]
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
