using Microsoft.JSInterop;

namespace Essenskasse.Client.Services {

    public class ThemeService {

        private readonly IJSRuntime jsRuntime;

        public ThemeService(IJSRuntime jsRuntime) {
            this.jsRuntime = jsRuntime;
        }

        private ColorCode themeCode = ColorCode.Light;
        
        public ColorCode ThemeCode {
            get { return themeCode; }
            set {
                jsRuntime.InvokeVoidAsync("ChangeTheme", value.ToString(), themeCode.ToString());
                themeCode = value;
            }
        }

    }
}
