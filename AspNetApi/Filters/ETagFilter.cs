using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;

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

        public virtual string ComputeHash(object value)
        {
            var serialized = JsonSerializer.Serialize(value);
            var valueBytes = KeyDerivation.Pbkdf2(
                             password: serialized,
                             salt: Encoding.UTF8.GetBytes(Salt),
                             prf: KeyDerivationPrf.HMACSHA512,
                             iterationCount: 10000,
                             numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
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

            // generates ETag from the entire response Content
            var etag = ComputeHash(result);

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
            response.Headers.Add(HeaderNames.CacheControl, "private");
        }
        
        private const string Salt = "Qco52Dtp9SBbq3DkBYmhWVYgy64YIMtq";
    }
}
