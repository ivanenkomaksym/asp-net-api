using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using AspNetApi.Services;

namespace AspNetApi.Filters
{
    // prevents the action filter methods to be invoked twice
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ETagFilter : ActionFilterAttribute, IAsyncActionFilter
    {
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext executingContext,
            ActionExecutionDelegate next
        )
        {
            var request = executingContext.HttpContext.Request;

            var executedContext = await next();
            var response = executedContext.HttpContext.Response;

            // Computing ETags for Response Caching on GET requests
            if (
                request.Method == HttpMethod.Get.Method
                && response.StatusCode == (int)HttpStatusCode.OK
            )
            {
                ValidateETagForResponseCaching(executedContext);
            }
        }

        private void ValidateETagForResponseCaching(ActionExecutedContext executedContext)
        {
            if (executedContext.Result == null)
            {
                return;
            }

            var request = executedContext.HttpContext.Request;
            var response = executedContext.HttpContext.Response;

            var result = (executedContext.Result as ObjectResult).Value;

            // generate ETag from LastModified property
            //var etag = GenerateEtagFromLastModified(result.LastModified);

            // generates ETag from the entire response Content
            var etag = ETagService.ComputeWithHashFunction(result);

            if (request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
            {
                // fetch etag from the incoming request header
                var incomingEtag = request.Headers[HeaderNames.IfNoneMatch].ToString();

                // if both the etags are equal
                // raise a 304 Not Modified Response
                if (incomingEtag.Equals(etag))
                {
                    executedContext.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
                }
            }

            // add ETag response header
            response.Headers.Add(HeaderNames.ETag, new[] { etag });
        }
    }
}
