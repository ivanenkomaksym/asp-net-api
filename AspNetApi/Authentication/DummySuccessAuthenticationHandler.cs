using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AspNetApi.Authentication
{
    /// <summary>
    /// This authentication scheme will always succeed if Authorization header is present. Content is not checked. Used for health checks.
    /// </summary>
    public class DummySuccessAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DummySuccessAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                                  ILoggerFactory logger,
                                                  UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        public static string Name = "DummyScheme";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing authorization"));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, "AuthenticatedUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}