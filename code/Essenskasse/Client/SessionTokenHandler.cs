using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace Essenskasse.Client {

    /// <summary>Haengt an jede Anfrage an die API das Sitzungs-Token des angemeldeten Benutzers
    /// als <c>Authorization: Bearer {Token}</c> an. Das Token liegt nach dem Login im LocalStorage
    /// (Schluessel "SessionToken"). Anfragen an andere Server erhalten es nie.</summary>
    public class SessionTokenHandler : DelegatingHandler {

        private readonly ILocalStorageService _localStorage;
        private readonly IConfiguration _configuration;

        public SessionTokenHandler(ILocalStorageService localStorage, IConfiguration configuration) {
            _localStorage = localStorage;
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (request.Headers.Authorization is null && IsApiRequest(request.RequestUri)) {
                try {
                    Guid token = await _localStorage.GetItemAsync<Guid>("SessionToken");
                    if (token != Guid.Empty) {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.ToString());
                    }
                }
                catch {
                    // Ohne lesbares Token geht die Anfrage anonym raus; die API antwortet dann ggf. mit 401.
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }

        private bool IsApiRequest(Uri? requestUri) {
            if (requestUri is null || !requestUri.IsAbsoluteUri) {
                return false;
            }
            if (!Uri.TryCreate(_configuration["API-Adress"], UriKind.Absolute, out Uri? apiUri)) {
                return false;
            }
            return requestUri.Scheme == apiUri.Scheme && requestUri.Host == apiUri.Host && requestUri.Port == apiUri.Port;
        }
    }
}
