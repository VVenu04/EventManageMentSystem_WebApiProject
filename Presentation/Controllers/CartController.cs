using Application.Common; // ApiResponse-க்காக
using Application.DTOs.Cart;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class CartController : BaseApiController 
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("User not identified."));

            dto.CustomerID = CurrentUserId;

            try
            {
                var cart = await _cartService.AddToCartAsync(dto);
                return Ok(ApiResponse<object>.Success(cart, "Item added to cart successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("User not identified."));

            try
            {
                var cart = await _cartService.GetMyCartAsync(CurrentUserId);
                return Ok(ApiResponse<object>.Success(cart));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        [HttpDelete("remove/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid itemId)
        {
            try
            {
                await _cartService.RemoveFromCartAsync(itemId);
                return Ok(ApiResponse<object?>.Success(null, "Item removed from cart."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

       
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("User not identified."));

            try
            {
                // Service-ல் CheckoutAsync மெதட் BookingID-ஐ ரிட்டர்ன் செய்யும்படி மாற்ற வேண்டும்
                // அல்லது Cart-ஐ Pending-ஆக மாற்றிய பின், அந்த Booking Object-ஐ ரிட்டர்ன் செய்யவும்.

                var bookingId = await _cartService.CheckoutAsync(CurrentUserId); // Service-ஐயும் மாற்ற வேண்டும்

                // 🚨 FIX: Return BookingID so Frontend can go to Payment Page
                return Ok(ApiResponse<object>.Success(new { bookingId }, "Checkout successful."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

    }
}