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
    public class BookingController : BaseApiController
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

            // 1. Auth Check
            if (CurrentUserId == Guid.Empty)
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
                var newBooking = await _bookingService.CreateBookingAsync(createBookingDto, CurrentUserId);

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
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token"));

            if (id == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Booking ID."));

            try
            {
                await _bookingService.CancelBookingAsync(id, CurrentUserId);

                // Success Response (No data needed, just message)
                return Ok(ApiResponse<object>.Success(null, "Booking cancelled and payment refunded successfully."));
            }
            catch (Exception ex)
            {
                // Example, "Cancellation period expired" or "Unauthorized to cancel"
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        [HttpGet("vendor/{vendorId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingConfirmationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookingsByVendor(Guid vendorId)
        {
            try
            {
                if (vendorId == Guid.Empty)
                    return BadRequest(ApiResponse<object>.Failure("Invalid Vendor ID"));

                var bookings = await _bookingService.GetBookingsByVendorAsync(vendorId);

                return Ok(ApiResponse<IEnumerable<BookingConfirmationDto>>.Success(bookings ?? new List<BookingConfirmationDto>()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }
        [HttpGet("my-bookings")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingConfirmationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBookings()
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token"));

            try
            {
                var bookings = await _bookingService.GetBookingsByCustomerAsync(CurrentUserId);

                return Ok(ApiResponse<IEnumerable<BookingConfirmationDto>>.Success(bookings ?? new List<BookingConfirmationDto>()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }



        // PUT: api/Booking/track-status
        [HttpPut("track-status")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateTracking([FromBody] UpdateTrackingDto dto)
        {
            try
            {
                await _bookingService.UpdateTrackingStatusAsync(dto, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Status updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // POST: api/Booking/complete-job
        [HttpPost("complete-job")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> CompleteJob([FromBody] CompleteJobDto dto)
        {
            try
            {
                var success = await _bookingService.CompleteServiceAsync(dto, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Job completed and verified successfully!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }



        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingConfirmationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookingsByCustomer(Guid customerId)
        {
            if (customerId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Failure("Invalid Customer ID."));

            try
            {
                var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);

                return Ok(ApiResponse<IEnumerable<BookingConfirmationDto>>.Success(bookings ?? new List<BookingConfirmationDto>()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }
    }


}



