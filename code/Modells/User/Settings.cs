using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Modells.User {

    /// <summary></summary>
    public class Settings {

        /// <summary>The value describes the user's preference as to whether the
        /// dashboard displays two lists of people or an alphabetical list.</summary>
        [JsonInclude, JsonPropertyName("IsSingleListWishDashboard"), JsonProperty(nameof(IsSingleListWishDashboard))]
        public bool IsSingleListWishDashboard { get; set; } = false;

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("LoadAmountPayment"), JsonProperty(nameof(LoadAmountPayment))]
        public int LoadAmountPayment { get; set; } = 10;

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("LoadAmountOrder"), JsonProperty(nameof(LoadAmountOrder))]
        public int LoadAmountOrder { get; set; } = 25;

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("FilterShowProvider"), JsonProperty(nameof(FilterShowProvider))]
        public bool FilterShowProvider { get; set; } = true;

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("DashboardMoneyLimit"), JsonProperty(nameof(DashboardMoneyLimit))]
        public Money DashboardMoneyLimit { get; set; } = new Money(0, 0);

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("FilterShowPerson"), JsonProperty(nameof(FilterShowPerson))]
        public bool FilterShowPerson { get; set; } = true;

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("AccessToken"), JsonProperty(nameof(AccessToken))]
        public Guid? AccessToken { get; set; } = null;

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("AdminEmail"), JsonProperty(nameof(AdminEmail))]
        public string AdminEmail { get; set; } = "";

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("NoticeableOnRightSide"), JsonProperty(nameof(NoticeableOnRightSide))]
        public bool NoticeableOnRightSide { get; set; } = true;

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("SendConfirmationMail"), JsonProperty(nameof(SendConfirmationMail))]
        public ConfirmationMailMode SendConfirmationMail { get; set; } = ConfirmationMailMode.Optional;

        /// <summary></summary>
        public Settings() { }

        /// <summary></summary>
        /// <param name="isSingleListWishDashboard"></param>
        /// <param name="loadAmountPayment"></param>
        /// <param name="loadAmountOrder"></param>
        /// <param name="filterShowProvider"></param>
        /// <param name="dashboardMoneyLimit"></param>
        /// <param name="filterShowPerson"></param>
        /// <param name="accessToken"></param>
        /// <param name="adminEmail"></param>
        /// <param name="noticeableOnRightSide"></param>
        /// <param name="sendConfirmationMail"></param>
        [System.Text.Json.Serialization.JsonConstructor]
        public Settings(bool isSingleListWishDashboard, int loadAmountPayment, int loadAmountOrder, bool filterShowProvider, Money dashboardMoneyLimit,
            bool filterShowPerson, Guid? accessToken, string adminEmail, bool noticeableOnRightSide, ConfirmationMailMode sendConfirmationMail) {
            IsSingleListWishDashboard = isSingleListWishDashboard;
            LoadAmountPayment = loadAmountPayment;
            LoadAmountOrder = loadAmountOrder;
            FilterShowProvider = filterShowProvider;
            DashboardMoneyLimit = dashboardMoneyLimit;
            FilterShowPerson = filterShowPerson;
            AccessToken = accessToken;
            AdminEmail = adminEmail;
            NoticeableOnRightSide = noticeableOnRightSide;
            SendConfirmationMail = sendConfirmationMail;
        }
    }
}
