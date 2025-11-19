using Application.DTOs.Booking;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // POST: api/booking
        [HttpPost]
        [Authorize(Roles = "Customer")] // Customer மட்டும் தான் Book செய்ய முடியும்
        public async Task<ActionResult<BookingConfirmationDto>> CreateBooking(CreateBookingDto createBookingDto)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == Guid.Empty)
            {
                return Unauthorized("Invalid customer token");
            }

            try
            {
                var newBooking = await _bookingService.CreateBookingAsync(createBookingDto, customerId);
                return Ok(newBooking);
                // அல்லது CreatedAtAction("GetBooking", new { id = newBooking.BookingID }, newBooking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // e.g., "Service not found"
            }
        }

        // GET: api/booking/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Customer,Vendor,Admin")] // எல்லோரும் பார்க்கலாம் (Owner-ஆ என check செய்ய வேண்டும்)
        public async Task<ActionResult<BookingConfirmationDto>> GetBooking(Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // (Security Check: இந்த Booking, login செய்த Customer-க்குச் சொந்தமானதா என check செய்ய வேண்டும்)

            return Ok(booking);
        }


        // --- Helper Method ---
        private Guid GetCurrentCustomerId()
        {
            // Token-இல் உள்ள 'NameId' claim-ஐப் பெறு
            var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (customerIdClaim != null && Guid.TryParse(customerIdClaim.Value, out Guid customerId))
            {
                return customerId;
            }
            return Guid.Empty;
        }
    }
}
