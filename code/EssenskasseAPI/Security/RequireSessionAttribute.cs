using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EssenskasseAPI.Security {

    /// <summary>Verlangt eine gueltige Sitzung (<c>Authorization: Bearer {Token}</c>).
    /// Mit <see cref="AdminOnly"/> ist zusaetzlich die Administrator-Rolle noetig.
    /// <see cref="AllowAnonymousAttribute"/> an einer Methode hebt die Pflicht fuer diese Methode auf.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RequireSessionAttribute : Attribute, IAsyncAuthorizationFilter {

        /// <summary>Nur Administratoren (siehe <see cref="SessionAuth.IsAdmin"/>) duerfen aufrufen.</summary>
        public bool AdminOnly { get; set; } = false;

        /// <inheritdoc/>
        public Task OnAuthorizationAsync(AuthorizationFilterContext context) {
            if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()) {
                return Task.CompletedTask;
            }

            SessionInfo? session = SessionAuth.Authenticate(context.HttpContext);
            if (session is null) {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
            else if (AdminOnly && !SessionAuth.IsAdmin(context.HttpContext, session)) {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            return Task.CompletedTask;
        }
    }
}
