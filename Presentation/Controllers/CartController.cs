using Application.DTOs.Cart;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        
        
       
            private readonly ICartService _cartService;

            public CartController(ICartService cartService)
            {
                _cartService = cartService;
            }

            [HttpPost("add")]
            public async Task<IActionResult> AddToCart(AddToCartDto dto)
            {
                dto.CustomerID = GetCurrentUserId(); // Helper method
                var cart = await _cartService.AddToCartAsync(dto);
                return Ok(cart);
            }

            [HttpGet]
            public async Task<IActionResult> GetMyCart()
            {
                var userId = GetCurrentUserId();
                var cart = await _cartService.GetMyCartAsync(userId);
                return Ok(cart);
            }

            [HttpDelete("remove/{itemId}")]
            public async Task<IActionResult> RemoveItem(Guid itemId)
            {
                await _cartService.RemoveFromCartAsync(itemId);
                return Ok("Item removed");
            }

            [HttpPost("checkout")]
            public async Task<IActionResult> Checkout()
            {
                var userId = GetCurrentUserId();
                await _cartService.CheckoutAsync(userId);
                return Ok("Checkout successful. Proceed to payment.");
            }
            private Guid GetCurrentUserId()
            {
                // Token-இல் உள்ள 'NameId' claim-ஐப் பெறு
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return userId;
                }
                return Guid.Empty;
            }
        
    }
}
