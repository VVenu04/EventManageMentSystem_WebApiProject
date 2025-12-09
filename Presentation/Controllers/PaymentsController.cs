using Application.Common; 
using Application.DTOs.Payment;
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
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST: api/payments/create-intent
        [HttpPost("create-intent")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequestDto dto)
        {
            if (dto == null || dto.BookingID == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Failure("Invalid booking request."));
            }

            try
            {
                var clientSecret = await _paymentService.CreatePaymentIntentAsync(dto);
                // Return the secret inside the Data object
                return Ok(ApiResponse<object>.Success(new { clientSecret }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // POST: api/payments/confirm
        [HttpPost("confirm")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.PaymentIntentId))
            {
                return BadRequest(ApiResponse<object>.Failure("Invalid payment confirmation details."));
            }

            try
            {
                var success = await _paymentService.ConfirmPaymentAndDistributeFundsAsync(dto.PaymentIntentId);

                if (!success)
                {
                    return BadRequest(ApiResponse<object>.Failure("Payment confirmation failed or Payment not successful yet."));
                }

                return Ok(ApiResponse<object>.Success(null, "Payment successful! Booking confirmed and funds distributed."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }
        [HttpGet("wallet-history")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<WalletTransactionDto>>> GetWalletHistory()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized();

            var history = await _paymentService.GetCustomerWalletHistoryAsync(CurrentUserId);
            return Ok(ApiResponse<IEnumerable<WalletTransactionDto>>.Success(history));
        }

    }
}