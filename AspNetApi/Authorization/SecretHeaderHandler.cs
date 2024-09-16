using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    /// <summary>
    /// Custom authorization handler that will check if secret header with correct value is present in the request.
    /// </summary>
    public class SecretHeaderHandler : AuthorizationHandler<SecretHeaderRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SecretHeaderRequirement requirement)
        {
            var httpContext = (context.Resource as HttpContext);

            if (httpContext != null && httpContext.Request.Headers.TryGetValue(requirement.HeaderName, out var headerValue))
            {
                if (headerValue == requirement.ExpectedValue)
                {
                    context.Succeed(requirement); // Authorization succeeds
                    return Task.CompletedTask;
                }
            }

            // Fail if header is missing or value is incorrect
            var failureReason = new SecretHeaderFailureReason(this, "Invalid secret header value.");
            context.Fail(failureReason);
            return Task.CompletedTask;
        }
    }
}
