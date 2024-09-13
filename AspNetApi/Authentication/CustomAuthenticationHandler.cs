using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AspNetApi.Authentication
{
    public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public CustomAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                           ILoggerFactory logger,
                                           UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("secret_header", out var headerValue))
            {
                return Task.FromResult(AuthenticateResult.Fail("Header not found."));
            }

            if (headerValue != "expected_value")
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid header value."));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, "HealthCheckUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

}
