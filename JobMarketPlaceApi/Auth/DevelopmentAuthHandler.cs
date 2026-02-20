// JobMarketPlaceApi\Auth\DevelopmentAuthHandler.cs
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace JobMarketPlaceApi.Auth
{
    // DEVELOPMENT ONLY: simple auth handler that creates an authenticated principal for every request.
    // Replace with a real scheme (JwtBearer, Identity, etc.) in production.
    public class DevelopmentAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DevelopmentAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Create a simple identity. Add claims as needed (roles, name, id).
            var claims = new[] { new Claim(ClaimTypes.Name, "dev-user") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}