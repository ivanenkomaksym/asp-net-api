using Microsoft.AspNetCore.Authorization;

namespace AspNetApi.Authorization
{
    public class MinimumAgeRequirement(string headerName, ushort minimumAge) : IAuthorizationRequirement
    {
        public string HeaderName = headerName;
        public ushort MinimumAge = minimumAge;
    }
}
