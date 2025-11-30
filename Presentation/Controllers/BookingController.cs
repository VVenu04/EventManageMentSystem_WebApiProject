using Application.Common; 
using Application.DTOs.Booking;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // POST: Create Booking 
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<BookingConfirmationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BookingConfirmationDto>> CreateBooking(CreateBookingDto createBookingDto)
        {
            var customerId = GetCurrentCustomerId();

            // 1. Auth Check
            if (customerId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid customer token."));
            }

            // 2. Validation Check
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<BookingConfirmationDto>.Failure("Validation failed.", errors));
            }

            try
            {
                var newBooking = await _bookingService.CreateBookingAsync(createBookingDto, customerId);

                // 3. Success Response
                return CreatedAtAction(
                    nameof(GetBooking),
                    new { id = newBooking.BookingID },
                    ApiResponse<BookingConfirmationDto>.Success(newBooking, "Booking created successfully.")
                );
            }
            catch (Exception ex)
            {
                // Example "Service not found", "Date already booked", "Expired date"
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // GET: Get Booking
        [HttpGet("{id}")]
        [Authorize(Roles = "Customer,Vendor,Admin")]
        [ProducesResponseType(typeof(ApiResponse<BookingConfirmationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingConfirmationDto>> GetBooking(Guid id)
        {
            if (id == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Booking ID."));

            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);

                if (booking == null)
                {
                    return NotFound(ApiResponse<object>.Failure("Booking not found."));
                }

                return Ok(ApiResponse<BookingConfirmationDto>.Success(booking));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  PUT: Cancel Booking 
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token"));

            if (id == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Booking ID."));

            try
            {
                await _bookingService.CancelBookingAsync(id, customerId);

                // Success Response (No data needed, just message)
                return Ok(ApiResponse<object>.Success(null, "Booking cancelled and payment refunded successfully."));
            }
            catch (Exception ex)
            {
                // Example, "Cancellation period expired" or "Unauthorized to cancel"
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  Helper Method 
        private Guid GetCurrentCustomerId()
        {
            var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (customerIdClaim != null && Guid.TryParse(customerIdClaim.Value, out Guid customerId))
            {
                return customerId;
            }
            return Guid.Empty;
        }
    }
}