using AspNetApi.Filters;
using AspNetApi.Models;
using AspNetApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AspNetApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(IShoppingCartRepository shoppingCartRepository, ILogger<ShoppingCartController> logger)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _logger = logger;
        }

        [HttpGet(Name = "GetAllShoppingCarts")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        [ETagFilter]
        public async Task<ActionResult<IEnumerable<ShoppingCart>>> GetAll()
        {
            var shoppingCarts = await _shoppingCartRepository.GetShoppingCarts();

            return shoppingCarts;
        }

        [HttpGet("{customerId}", Name = "GetShoppingCart")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        [VersionBasedETagFilter]
        public async Task<ActionResult<ShoppingCart>> Get(Guid customerId)
        {
            var shoppingCart = await _shoppingCartRepository.GetShoppingCart(customerId);

            if (shoppingCart == null)
                return NotFound();

            return shoppingCart;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateShoppingCart([FromBody] ShoppingCart cart)
        {
            await _shoppingCartRepository.CreateShoppingCart(cart);

            return CreatedAtRoute("GetShoppingCart", new { customerId = cart.CustomerId }, cart);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateShoppingCart([FromBody] ShoppingCart cart)
        {
            var result = await _shoppingCartRepository.UpdateShoppingCart(cart);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{customerId}")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteProductById(Guid customerId)
        {
            var result = await _shoppingCartRepository.DeleteShoppingCart(customerId);
            if (!result)
                return NotFound();

            return NoContent();
        }


        [HttpPost("Checkout")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<bool>> Checkout(Guid customerId)
        {
            return await _shoppingCartRepository.Checkout(customerId);
        }
    }
}
