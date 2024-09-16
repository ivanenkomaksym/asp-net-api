using AspNetApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using NSubstitute;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace AspNetApi.IntegrationTests.UnitTests
{
    public class CustomAuthorizationResultTransformerTests
    {
        private readonly SecretHeaderAuthorizationResultTransformer _transformer = new();

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
            Assert.Equal("Authentication required.", responseContent);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturn403_WhenAuthorizationFails()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "TestUser") }, "TestAuthScheme")); // Authenticated
            var next = Substitute.For<RequestDelegate>();
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            // Simulate an authorization failure with a custom reason
            var failureReason = new AuthorizationFailureReason(null, "Invalid secret header");
            var authorizeResult = PolicyAuthorizationResult.Forbid(AuthorizationFailure.Failed(new[] { failureReason }));

            // Act
            await _transformer.HandleAsync(next, context, policy, authorizeResult);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            var responseContent = GetResponseBody(context.Response);
            Assert.Equal("Authorization failed: Invalid secret header", responseContent);
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
