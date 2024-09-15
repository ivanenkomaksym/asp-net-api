using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AspNetApi.Authentication
{
    /// <summary>
    /// This authentication scheme will always succeed. Used for health checks.
    /// </summary>
    public class AlwaysSuccessAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AlwaysSuccessAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                                  ILoggerFactory logger,
                                                  UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "HealthCheckUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}