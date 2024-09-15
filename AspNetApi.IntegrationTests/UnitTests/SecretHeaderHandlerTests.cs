using AspNetApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Xunit;

namespace AspNetApi.IntegrationTests.UnitTests
{
    public sealed class SecretHeaderHandlerTests
    {
        private const string HeaderName = "X-Secret-Header";
        private const string ExpectedValue = "secret-value";

        [Fact]
        public async Task HandleRequirementAsync_ShouldSucceed_WhenHeaderIsPresentAndValueIsCorrect()
        {
            // Arrange
            var requirement = new SecretHeaderRequirement(HeaderName, ExpectedValue);
            var handler = new SecretHeaderHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderName] = ExpectedValue;

            var authorizationContext = CreateAuthorizationHandlerContext(requirement, httpContext);

            // Act
            await handler.HandleAsync(authorizationContext);

            // Assert
            Assert.True(authorizationContext.HasSucceeded, "Authorization should succeed when header value is correct.");
            Assert.False(authorizationContext.HasFailed, "Authorization should not fail when header value is correct.");
        }

        [Fact]
        public async Task HandleRequirementAsync_ShouldFail_WhenHeaderIsMissing()
        {
            // Arrange
            var requirement = new SecretHeaderRequirement(HeaderName, ExpectedValue);
            var handler = new SecretHeaderHandler();

            var httpContext = new DefaultHttpContext();
            // No header is added here

            var authorizationContext = CreateAuthorizationHandlerContext(requirement, httpContext);

            // Act
            await handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded, "Authorization should not succeed when header is missing.");
            Assert.True(authorizationContext.HasFailed, "Authorization should fail when header is missing.");
        }

        [Fact]
        public async Task HandleRequirementAsync_ShouldFail_WhenHeaderValueIsIncorrect()
        {
            // Arrange
            var requirement = new SecretHeaderRequirement(HeaderName, ExpectedValue);
            var handler = new SecretHeaderHandler();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderName] = "wrong-value";

            var authorizationContext = CreateAuthorizationHandlerContext(requirement, httpContext);

            // Act
            await handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded, "Authorization should not succeed when header value is incorrect.");
            Assert.True(authorizationContext.HasFailed, "Authorization should fail when header value is incorrect.");
        }

        private AuthorizationHandlerContext CreateAuthorizationHandlerContext(SecretHeaderRequirement requirement, HttpContext httpContext)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var resource = httpContext;

            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                resource
            );
        }
    }
}
