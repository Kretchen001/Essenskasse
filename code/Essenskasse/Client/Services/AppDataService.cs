namespace Essenskasse.Client.Services {

    /// <summary> Hold data for the hole application. </summary>
    public class AppDataService {

        /// <summary> Hold the current theme, from LocalStorage or user selection. </summary>
        public string ColorTheme { get; set; } = "light-theme";
    }
}
