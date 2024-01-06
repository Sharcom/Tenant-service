using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Tenant_service.AuthConfig
{
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }
        public string Scope { get; }

        public HasScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }

    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
                return Task.CompletedTask;

            Claim? scopeString = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer);

            if (scopeString == null)
                throw new FormatException("Scopes not found");

            // Split the scopes string into an array
            string[] scopes = scopeString.Value.Split(' ');

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s == requirement.Scope))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class ManagementAPIConfig
    {
        public ManagementAPIConfig() { }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string Domain { get; set; }
        public string Audiance { get; set; }
    }
}