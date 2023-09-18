using AspNetApi.Models;

namespace AspNetApi.Filters
{
    // prevents the action filter methods to be invoked twice
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class VersionBasedETagFilter : ETagFilter
    {
        public override string ComputeHash(object value)
        {
            var cart = value as ShoppingCart;
            return base.ComputeHash(cart.Version);
        }
    }
}
