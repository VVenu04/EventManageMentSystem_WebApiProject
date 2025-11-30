using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface IBookingRepository
    {
        Task<bool> IsServiceBookedOnDateAsync(Guid serviceId, DateTime eventDate);
        Task<Booking> AddAsync(Booking booking);

        Task<Booking> GetByIdAsync(Guid bookingId);
        Task<int> GetBookingCountForServiceOnDateAsync(Guid serviceId, DateTime eventDate);
        Task UpdateAsync(Booking booking);

        Task<Booking> GetCartByCustomerIdAsync(Guid customerId); // Status = "Cart" உள்ளதை மட்டும் எடு
        Task AddItemToCartAsync(BookingItem item);
        Task RemoveItemFromCartAsync(Guid itemId);
    }
}
