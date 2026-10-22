using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Essenskasse.Client {

    public class AuthStateService : AuthenticationStateProvider {

        public static bool IsAuthenticated { get; set; }
        public static bool IsAuthenticating { get; set; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
            ClaimsIdentity identity;

            if (IsAuthenticating) {
                return null;
            }
            else if (IsAuthenticated) {
                identity = new ClaimsIdentity(new List<Claim>
                            {
                            new Claim(ClaimTypes.Name, "TestUser")

                        }, "WebApiAuth");
            }
            else {
                identity = new ClaimsIdentity();
            }

            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        /// <summary>Wird ausgeloest, wenn eine Seite den Anmeldedialog oeffnen moechte (z. B. "Zugriff verweigert").</summary>
        public event Action? LoginRequested;

        public void RequestLogin() {
            LoginRequested?.Invoke();
        }

        public void NotifyAuthenticationStateChanged() {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
