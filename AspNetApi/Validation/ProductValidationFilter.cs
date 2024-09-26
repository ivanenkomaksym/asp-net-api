using AspNetApi.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspNetApi.Validation
{
    public class ProductValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Extract the Product from the action arguments (assuming it comes from the request body)
            if (context.ActionArguments.TryGetValue("product", out var productObj) && productObj is Product product)
            {
                // Validate the Product itself
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(product);
                bool isValidProduct = Validator.TryValidateObject(product, validationContext, validationResults, validateAllProperties: true);

                // Validate CategoryInfo if it exists
                if (product.CategoryInfo != null)
                {
                    var categoryValidationContext = new ValidationContext(product.CategoryInfo);
                    bool isValidCategory = Validator.TryValidateObject(product.CategoryInfo, categoryValidationContext, validationResults, validateAllProperties: true);

                    // If category validation failed, mark the overall result as invalid
                    isValidProduct = isValidProduct && isValidCategory;
                }

                // If validation failed, return BadRequest with the validation errors
                if (!isValidProduct)
                {
                    var errorResponse = new ValidationProblemDetails();
                    foreach (var validationResult in validationResults)
                    {
                        foreach (var memberName in validationResult.MemberNames)
                        {
                            errorResponse.Errors.Add(memberName, [validationResult.ErrorMessage]);
                        }
                    }
                    context.Result = new BadRequestObjectResult(errorResponse);
                    return;
                }
            }

            // Continue to the next action if validation succeeds
            await next();
        }
    }
}
