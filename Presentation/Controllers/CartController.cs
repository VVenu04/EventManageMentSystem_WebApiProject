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
                return Ok(ApiResponse<object>.Success(null, "Item removed from cart."));
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
                await _cartService.CheckoutAsync(CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Checkout successful. Proceed to payment."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

    }
}