using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AspNetApi.Authentication
{
    /// <summary>
    /// Allows anonymous users. Used when <see cref="AspNetApi.Configuration.AuthenticationOptions.Enable"/> is false.
    /// </summary>
    public class AllowAnonymousAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AllowAnonymousAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                                   ILoggerFactory logger,
                                                   UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        public const string Name = "AllowAnonymousScheme";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Simulate an always authenticated result.
            var claims = new[] { new Claim(ClaimTypes.Name, "AnonymousUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
