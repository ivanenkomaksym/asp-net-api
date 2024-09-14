using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    public class SecretHeaderRequirement(string headerName, string expectedValue) : IAuthorizationRequirement
    {
        public string HeaderName { get; } = headerName;
        public string ExpectedValue { get; } = expectedValue;
    }

}
