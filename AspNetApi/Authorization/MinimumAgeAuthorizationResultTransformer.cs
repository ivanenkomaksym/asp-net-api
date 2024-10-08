﻿using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    public class MinimumAgeAuthorizationResultTransformer : IAuthorizationMiddlewareResultHandler
    {
        public async Task HandleAsync(RequestDelegate next,
                                      HttpContext context,
                                      AuthorizationPolicy policy,
                                      PolicyAuthorizationResult authorizeResult)
        {
            // If the authorization failed
            if (!authorizeResult.Succeeded)
            {
                var minimumAgeFailureReason = authorizeResult.AuthorizationFailure?.FailureReasons
                    .OfType<MinimumAgeFailureReason>()
                    .FirstOrDefault();
                if (minimumAgeFailureReason != null)
                {
                    // Handle authorization failure with custom message
                    var failureReason = minimumAgeFailureReason.Message;
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync($"Authorization failed: {failureReason}");
                }
            }
        }
    }
}
