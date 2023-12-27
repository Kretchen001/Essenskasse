namespace Essenskasse.Client.Services {

    /// <summary>Hold data for the hole application.</summary>
    public class AppDataService {

        /// <summary>The value describes the user's preference as to whether the
        /// dashboard displays two lists of people or an alphabetical list.</summary>
        public bool IsSingleListWishDashboard { get; set; } = false;
    }
}
