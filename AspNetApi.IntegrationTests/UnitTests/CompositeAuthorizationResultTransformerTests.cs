using AspNetApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using NSubstitute;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace AspNetApi.UnitTests
{
    public class CompositeAuthorizationResultTransformerTests
    {
        private readonly CompositeAuthorizationResultTransformer _transformer = new([new SecretHeaderAuthorizationResultTransformer(), new MinimumAgeAuthorizationResultTransformer()]);

        [Fact]
        public async Task HandleAsync_ShouldReturn401_WhenAuthenticationFails()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.User = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
            var next = Substitute.For<RequestDelegate>();
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            var authorizeResult = PolicyAuthorizationResult.Forbid();

            // Act
            await _transformer.HandleAsync(next, context, policy, authorizeResult);

            // Assert
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
            var responseContent = GetResponseBody(context.Response);
            Assert.Contains("Authentication required.", responseContent);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturn403_WhenInvalidSecretHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "TestUser") }, "TestAuthScheme")); // Authenticated
            var next = Substitute.For<RequestDelegate>();
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            // Simulate an authorization failure with a custom reason
            var secretHeaderFailureReason = new SecretHeaderFailureReason(null, "Invalid secret header");
            var authorizeResult = PolicyAuthorizationResult.Forbid(AuthorizationFailure.Failed(new[] { secretHeaderFailureReason }));

            // Act
            await _transformer.HandleAsync(next, context, policy, authorizeResult);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            var responseContent = GetResponseBody(context.Response);
            Assert.Equal("Authorization failed: Invalid secret header", responseContent);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturn403_WhenUnderage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "TestUser") }, "TestAuthScheme")); // Authenticated
            var next = Substitute.For<RequestDelegate>();
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            // Simulate an authorization failure with a custom reason
            var minimumAgeFailureReason = new MinimumAgeFailureReason(null, "Underage");
            var authorizeResult = PolicyAuthorizationResult.Forbid(AuthorizationFailure.Failed(new[] { minimumAgeFailureReason }));

            // Act
            await _transformer.HandleAsync(next, context, policy, authorizeResult);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            var responseContent = GetResponseBody(context.Response);
            Assert.Equal("Authorization failed: Underage", responseContent);
        }

        [Fact]
        public async Task HandleAsync_ShouldCallDefaultHandler_WhenAuthorizationSucceeds()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "TestUser") }, "TestAuthScheme")); // Authenticated
            var next = Substitute.For<RequestDelegate>();
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            var authorizeResult = PolicyAuthorizationResult.Success();

            // Act
            await _transformer.HandleAsync(next, context, policy, authorizeResult);

            // Assert
            await next.Received(1).Invoke(context);
        }

        private string GetResponseBody(HttpResponse response)
        {
            response.Body.Position = 0;
            using StreamReader reader = new(response.Body, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }

}
