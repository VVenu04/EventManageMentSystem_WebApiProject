using Application.Common;
using Application.DTOs.Cart;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto) 
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("User not identified."));

            dto.CustomerID = CurrentUserId;

            // Basic Validation
            if (dto.ServiceID == null && dto.PackageID == null)
            {
                return BadRequest(ApiResponse<object>.Failure("Must provide either a Service or Package ID."));
            }

            try
            {
                var cart = await _cartService.AddToCartAsync(dto);
                return Ok(ApiResponse<object>.Success(cart, "Item added to cart successfully."));
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(ApiResponse<object>.Failure(errorMessage));
            }
        }

        // ... (GetMyCart, RemoveItem, Checkout methods keep as is) ...
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("User not identified."));
            try
            {
                var cart = await _cartService.GetMyCartAsync(CurrentUserId);
                return Ok(ApiResponse<object>.Success(cart));
            }
            catch (Exception ex) { return BadRequest(ApiResponse<object>.Failure(ex.Message)); }
        }

        [HttpDelete("remove/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid itemId)
        {
            try
            {
                await _cartService.RemoveFromCartAsync(itemId);
                return Ok(ApiResponse<object?>.Success(null, "Item removed."));
            }
            catch (Exception ex) { return BadRequest(ApiResponse<object>.Failure(ex.Message)); }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("User not identified."));
            try
            {
                var bookingId = await _cartService.CheckoutAsync(CurrentUserId);
                return Ok(ApiResponse<object>.Success(new { bookingId }, "Checkout successful."));
            }
            catch (Exception ex) { return BadRequest(ApiResponse<object>.Failure(ex.Message)); }
        }
    }
}