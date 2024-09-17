using AspNetApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Xunit;

namespace AspNetApi.Tests.UnitTests
{
    public class MinimumAgeHandlerTests
    {
        [Fact]
        public async Task HandleRequirementAsync_ShouldSucceed_WhenAgeIsValid()
        {
            // Arrange
            var requirement = new MinimumAgeRequirement("X-Age", 18); // Assume the required header is "X-Age" and minimum age is 18

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Age"] = "20"; // Valid age

            var handler = new MinimumAgeHandler();

            var authorizationContext = CreateAuthorizationHandlerContext(requirement, httpContext);

            // Act
            await handler.HandleAsync(authorizationContext);

            // Assert
            Assert.True(authorizationContext.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_ShouldFail_WhenUnderage()
        {
            // Arrange
            var requirement = new MinimumAgeRequirement("X-Age", 18); // Assume the required header is "X-Age" and minimum age is 18

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Age"] = "16"; // Underage

            var handler = new MinimumAgeHandler();

            var authorizationContext = CreateAuthorizationHandlerContext(requirement, httpContext);

            // Act
            await handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded);
            Assert.Contains(authorizationContext.FailureReasons, r => r.Message == "Underage.");
        }

        [Fact]
        public async Task HandleRequirementAsync_ShouldFail_WhenAgeHeaderMissing()
        {
            // Arrange
            var requirement = new MinimumAgeRequirement("X-Age", 18); // Assume the required header is "X-Age" and minimum age is 18

            var httpContext = new DefaultHttpContext(); // No "X-Age" header

            var handler = new MinimumAgeHandler();

            var authorizationContext = CreateAuthorizationHandlerContext(requirement, httpContext);

            // Act
            await handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded);
            Assert.Contains(authorizationContext.FailureReasons, r => r.Message == "Missing age.");
        }

        [Fact]
        public async Task HandleRequirementAsync_ShouldFail_WhenAgeHeaderHasIncorrectFormat()
        {
            // Arrange
            var requirement = new MinimumAgeRequirement("X-Age", 18); // Assume the required header is "X-Age" and minimum age is 18

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Age"] = "invalid"; // Incorrect age format

            var handler = new MinimumAgeHandler();

            var authorizationContext = CreateAuthorizationHandlerContext(requirement, httpContext);

            // Act
            await handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded);
            Assert.Contains(authorizationContext.FailureReasons, r => r.Message == "Incorrect age format.");
        }

        private AuthorizationHandlerContext CreateAuthorizationHandlerContext(MinimumAgeRequirement requirement, HttpContext httpContext)
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
