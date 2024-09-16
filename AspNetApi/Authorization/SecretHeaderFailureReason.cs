using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    public class SecretHeaderFailureReason : AuthorizationFailureReason
    {
        public SecretHeaderFailureReason(IAuthorizationHandler handler, string message) : base(handler, message)
        {
        }
    }
}
