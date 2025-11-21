using Application.DTOs.Payment;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // --- 1. Create Payment Intent (Frontend: "Pay Now" பட்டனை அழுத்தும்போது) ---
        // இது Stripe-இடம் சொல்லி ஒரு Transaction-ஐத் தொடங்கும்.
        [HttpPost("create-intent")]
        [Authorize(Roles = "Customer")] // Customer மட்டும் தான் பணம் செலுத்த முடியும்
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequestDto dto)
        {
            if (dto == null || dto.BookingID == Guid.Empty)
            {
                return BadRequest("Invalid booking request.");
            }

            try
            {
                // Service-ஐ அழைத்து ClientSecret-ஐப் பெறுதல்
                var clientSecret = await _paymentService.CreatePaymentIntentAsync(dto);

                // Frontend-க்கு ClientSecret-ஐ அனுப்புதல் (இதை வைத்துதான் அவர்கள் Stripe-ல் பணம் கட்டுவார்கள்)
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                // e.g., "Booking not found" or Stripe errors
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 2. Confirm Payment (Frontend: Stripe-ல் பணம் கட்டிய பிறகு இதை call செய்யும்) ---
        // இது பணத்தைப் பிரித்து (10% vs 5%) Database-ல் பதிவு செய்யும்.
        [HttpPost("confirm")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.PaymentIntentId))
            {
                return BadRequest("Invalid payment confirmation details.");
            }

            try
            {
                // Service-ஐ அழைத்து பணத்தைப் பிரித்தல் & Database update
                var success = await _paymentService.ConfirmPaymentAndDistributeFundsAsync(dto.PaymentIntentId);

                if (!success)
                {
                    return BadRequest(new { message = "Payment confirmation failed or Payment not successful yet." });
                }

                return Ok(new { message = "Payment successful! Booking confirmed and funds distributed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
