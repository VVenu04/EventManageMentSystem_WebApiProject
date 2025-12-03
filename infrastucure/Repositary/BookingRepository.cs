using Application.Interface.IRepo;
using Domain.Entities;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> AddAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
        public async Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(Guid customerId)
        {
            return await _context.Bookings
                .Include(b => b.BookingItems).ThenInclude(bi => bi.Service)
                .Include(b => b.BookingItems).ThenInclude(bi => bi.Package)
                .Where(b => b.CustomerID == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Customer) 
                .Include(b => b.BookingItems) 
                    .ThenInclude(item => item.Service) 
                        .ThenInclude(service => service!.Vendor) 
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);
        }
        public async Task<bool> IsServiceBookedOnDateAsync(Guid serviceId, DateTime eventDate)
        {
            return await _context.BookingItems

                .Where(item => item.ServiceItemID == serviceId)

                .AnyAsync(item => item.Booking!.EventDate.Date == eventDate.Date);
        }
        public async Task<int> GetBookingCountForServiceOnDateAsync(Guid serviceId, DateTime eventDate)
        {
            // BookingItems table-ஐத் தேடு
            return await _context.BookingItems

                // 1. அந்த ServiceID-ஐக் கொண்ட Item-ஆ?
                .Where(item => item.ServiceItemID == serviceId)

                // 2. அந்த Item-உடைய Parent Booking அதே தேதியிலா உள்ளது?
                .Where(item => item.Booking!.EventDate.Date == eventDate.Date)

                // 3. அவற்றின் எண்ணிக்கையைக் கொடு
                .CountAsync();
        }
        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }
        public async Task<Booking?> GetCartByCustomerIdAsync(Guid customerId)
        {
            return await _context.Bookings
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Service)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Package)
                .FirstOrDefaultAsync(b => b.CustomerID == customerId && b.BookingStatus == "Cart");
        }

        public async Task AddItemToCartAsync(BookingItem item)
        {
            _context.BookingItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemFromCartAsync(Guid itemId)
        {
            var item = await _context.BookingItems.FindAsync(itemId);
            if (item != null)
            {
                // Booking TotalPrice-ஐக் குறைக்கவும்
                var booking = await _context.Bookings.FindAsync(item.BookingID);
                if (booking != null)
                {
                    booking.TotalPrice -= item.ItemPrice;
                }

                _context.BookingItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Booking>> GetBookingsByVendorAsync(Guid vendorId)
        {
            return await _context.Bookings
                .Include(b => b.Customer) // Customer பெயர் காட்ட
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Service) // Service பெயர் காட்ட
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.Package) // Package பெயர் காட்ட
                                                   // அந்த Vendor-க்குச் சொந்தமான Item ஏதேனும் உள்ளதா எனத் தேடுகிறது
                .Where(b => b.BookingItems.Any(bi => bi.VendorID == vendorId))
                .OrderByDescending(b => b.CreatedAt) // புதியது முதலில்
                .ToListAsync();
        }
    }
}

