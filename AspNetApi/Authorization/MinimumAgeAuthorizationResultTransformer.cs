﻿using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    public class MinimumAgeAuthorizationResultTransformer : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();

        public async Task HandleAsync(RequestDelegate next,
                                      HttpContext context,
                                      AuthorizationPolicy policy,
                                      PolicyAuthorizationResult authorizeResult)
        {
            // If the authorization failed
            if (!authorizeResult.Succeeded)
            {
                if (context.User?.Identity?.IsAuthenticated == false)
                {
                    // Handle authentication failure
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Authentication required.");
                    return;
                }

                if (authorizeResult.AuthorizationFailure?.FailureReasons.FirstOrDefault() is MinimumAgeFailureReason minimumAgeFailureReason)
                {
                    // Handle authorization failure with custom message
                    var failureReason = minimumAgeFailureReason.Message;
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync($"Authorization failed: {failureReason}");
                    return;
                }
            }
        }
    }
}
