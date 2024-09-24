using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    /// <summary>
    /// Transforms default response with <seealso cref="AuthorizationFailure"/> reason details.
    /// Uses failures set by <see cref="SecretHeaderHandler"/>.
    /// </summary>
    public class SecretHeaderAuthorizationResultTransformer : IAuthorizationMiddlewareResultHandler
    {
        public async Task HandleAsync(RequestDelegate next,
                                      HttpContext context,
                                      AuthorizationPolicy policy,
                                      PolicyAuthorizationResult authorizeResult)
        {
            // If the authorization failed
            if (!authorizeResult.Succeeded)
            {
                if (authorizeResult.AuthorizationFailure?.FailureReasons.FirstOrDefault() is SecretHeaderFailureReason secretHeaderFailureReason)
                {
                    // Handle authorization failure with custom message
                    var failureReason = secretHeaderFailureReason.Message;
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync($"Authorization failed: {failureReason}");
                }
            }
        }
    }
}