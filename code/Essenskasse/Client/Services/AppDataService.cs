using Modells;

namespace Essenskasse.Client.Services {

    /// <summary>Hold data for the hole application.</summary>
    public class AppDataService {

        /// <summary>The value describes the user's preference as to whether the
        /// dashboard displays two lists of people or an alphabetical list.</summary>
        public bool IsSingleListWishDashboard { get; set; } = false;

        /// <summary></summary>
        public int LoadAmountPayment { get; set; } = 10;

        /// <summary></summary>
        public int LoadAmountOrder { get; set; } = 25;

        /// <summary></summary>
        public bool FilterShowProvider { get; set; } = true;

        /// <summary></summary>
        public Money DashboardMoneyLimit { get; set; } = new Money(0, 0);

        /// <summary></summary>
        public bool FilterShowPerson { get; set; } = true;

        /// <summary></summary>
        public Guid? AccessToken { get; set; } = null;
    }
}
