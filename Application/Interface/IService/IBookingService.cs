using Application.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IBookingService
    {
        Task<BookingConfirmationDto> CreateBookingAsync(CreateBookingDto createBookingDto, Guid customerId);

        Task<BookingConfirmationDto> GetBookingByIdAsync(Guid bookingId);

        Task<IEnumerable<BookingConfirmationDto>> GetBookingsByVendorAsync(Guid vendorId);
        Task CancelBookingAsync(Guid bookingId, Guid customerId);
    }
}
