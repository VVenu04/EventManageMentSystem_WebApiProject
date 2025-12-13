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
        [HttpPost("process-mock")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto dto)
        {
            if (dto == null || dto.BookingID == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Failure("Invalid booking request."));
            }

            try
            {
                // Service returns bool (True = Success)
                var isSuccess = await _paymentService.ProcessMockPaymentAsync(dto.BookingID);

                if (isSuccess)
                {
                    return Ok(ApiResponse<object>.Success(null, "Payment processed successfully!"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.Failure("Payment failed. Please check your wallet balance."));
                }
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