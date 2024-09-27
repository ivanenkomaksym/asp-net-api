using AspNetApi.Models;
using AspNetApi.Validation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace AspNetApi.UnitTests
{
    public class ProductValidationFilterTests
    {
        [Fact]
        public async Task Should_Succeed_When_Product_Is_Valid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Sample Product",
                CategoryInfo = new BooksCategory
                {
                    NofPages = 300,
                    Authors = ["Author 1"]
                }
            };

            var context = new ActionExecutingContext(
                new ActionContext(
                    new DefaultHttpContext(),
                    new Microsoft.AspNetCore.Routing.RouteData(),
                    new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                new Dictionary<string, object> { { "product", product } },
                new object());

            var next = Substitute.For<ActionExecutionDelegate>();
            var filter = new ProductValidationFilter();
            // Act
            await filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Null(context.Result); // Ensure that no result was set, meaning validation passed
            await next.Received(1)(); // Ensure the next delegate was called
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_CategoryInfo_Is_Invalid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Sample Product",
                CategoryInfo = new BooksCategory
                {
                    NofPages = 300,
                    Authors = [] // Empty list, violates [MinLength(1)]
                }
            };

            var context = new ActionExecutingContext(
                new ActionContext(
                    new DefaultHttpContext(),
                    new Microsoft.AspNetCore.Routing.RouteData(),
                    new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                new Dictionary<string, object> { { "product", product } },
                new object());

            var next = Substitute.For<ActionExecutionDelegate>();
            var filter = new ProductValidationFilter();

            // Act
            await filter.OnActionExecutionAsync(context, next);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            var validationProblemDetails = Assert.IsType<ValidationProblemDetails>(result.Value);
            Assert.Contains("The field Authors must be a string or array type with a minimum length of '1'.", validationProblemDetails.Errors[nameof(BooksCategory.Authors)]);
        }
    }
}