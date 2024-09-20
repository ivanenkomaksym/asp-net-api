using AspNetApi.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using NSubstitute;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace AspNetApi.UnitTests
{
    public class CompositeAuthorizationResultTransformerTests
    {
        private readonly CompositeAuthorizationResultTransformer _transformer = new([new SecretHeaderAuthorizationResultTransformer(), new MinimumAgeAuthorizationResultTransformer()]);

        private class TestAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
        {
            public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ShouldRegister_CompositeAuthorizationResultTransformer_WithHandlers()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock handlers
            var handler1 = new SecretHeaderAuthorizationResultTransformer();
            var handler2 = new MinimumAgeAuthorizationResultTransformer();

            // Register mocks in the service provider
            services.AddSingleton(handler1);
            services.AddSingleton(handler2);

            // Act - Register the composite transformer with the mock handlers
            services.AddCompositeAuthorizationResultTransformer(typeof(SecretHeaderAuthorizationResultTransformer), typeof(MinimumAgeAuthorizationResultTransformer));

            var serviceProvider = services.BuildServiceProvider();

            // Get the CompositeAuthorizationResultTransformer instance
            var compositeTransformer = serviceProvider.GetService<IAuthorizationMiddlewareResultHandler>() as CompositeAuthorizationResultTransformer;

            // Assert - Verify composite transformer is registered
            Assert.NotNull(compositeTransformer);

            // Assert - Verify handlers are passed to the composite transformer
            var handlersField = typeof(CompositeAuthorizationResultTransformer)
                .GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Instance);

            var handlers = handlersField.GetValue(compositeTransformer) as IEnumerable<IAuthorizationMiddlewareResultHandler>;

            Assert.NotNull(handlers);
            Assert.Contains(handler1, handlers);
            Assert.Contains(handler2, handlers);
        }

        [Fact]
        public void Should_Throw_When_Type_Does_Not_Implement_IAuthorizationMiddlewareResultHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddCompositeAuthorizationResultTransformer(typeof(string)); // Invalid type
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<IAuthorizationMiddlewareResultHandler>());

            Assert.Equal("String does not implement IAuthorizationMiddlewareResultHandler.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Service_Is_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert - Missing service should throw an exception
            services.AddCompositeAuthorizationResultTransformer(typeof(TestAuthorizationMiddlewareResultHandler)); // Not registered
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<IAuthorizationMiddlewareResultHandler>());

            Assert.Equal($"Service for {nameof(TestAuthorizationMiddlewareResultHandler)} could not be found.", exception.Message);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturn403_WhenInvalidSecretHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Mock IAuthenticationService
            var authenticationService = Substitute.For<IAuthenticationService>();

            // Mock IServiceProvider and set up RequestServices
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authenticationService);

            context.RequestServices = serviceProvider;
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

            // Mock IAuthenticationService
            var authenticationService = Substitute.For<IAuthenticationService>();

            // Mock IServiceProvider and set up RequestServices
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authenticationService);

            context.RequestServices = serviceProvider;
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
            await next.Received(3).Invoke(context);
        }

        private string GetResponseBody(HttpResponse response)
        {
            response.Body.Position = 0;
            using StreamReader reader = new(response.Body, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }

}
