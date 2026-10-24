using Modells;
using U = Modells.User;

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

        /// <summary></summary>
        public string AdminEmail { get; set; } = "";

        /// <summary></summary>
        public bool NoticeableOnRightSide { get; set; } = true;
        
        /// <summary>Die Schnellbetraege (Chips) im Bestellfenster in Cent; mindestens einer.</summary>
        public List<long> QuickAmounts { get; set; } = [.. Modells.QuickAmounts.Default];

        /// <summary></summary>
        public U.ConfirmationMailMode SendConfirmationMail { get; set; } = U.ConfirmationMailMode.Optional;

        public U.Settings GetAppDataAsSettingsObject() {
            return new U.Settings(
                isSingleListWishDashboard: IsSingleListWishDashboard,
                loadAmountPayment: LoadAmountPayment,
                loadAmountOrder: LoadAmountOrder,
                filterShowProvider: FilterShowProvider,
                dashboardMoneyLimit: DashboardMoneyLimit,
                filterShowPerson: FilterShowPerson,
                accessToken: null, // der Aenderungstoken gehoert zum Benutzer und wird nicht mit den Einstellungen gespeichert
                adminEmail: AdminEmail,
                noticeableOnRightSide: NoticeableOnRightSide,
                sendConfirmationMail: SendConfirmationMail
                ) {
                QuickAmounts = [.. QuickAmounts]
            };
        }

        public void SettingsToAppData(U.Settings settings) {
            IsSingleListWishDashboard = settings.IsSingleListWishDashboard;
            LoadAmountPayment = settings.LoadAmountPayment;
            LoadAmountOrder = settings.LoadAmountOrder;
            FilterShowProvider = settings.FilterShowProvider;
            DashboardMoneyLimit = settings.DashboardMoneyLimit;
            FilterShowPerson = settings.FilterShowPerson;
            // Der Aenderungstoken wird bewusst nicht uebernommen (siehe GetAppDataAsSettingsObject).
            AdminEmail = settings.AdminEmail;
            NoticeableOnRightSide = settings.NoticeableOnRightSide;
            SendConfirmationMail = settings.SendConfirmationMail;
        }
    }
}
