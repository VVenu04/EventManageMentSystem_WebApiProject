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
        // 1. UPDATE: Added check for Cancelled Status
      
        public async Task<bool> IsServiceBookedOnDateAsync(Guid serviceId, DateTime eventDate)
        {
            return await _context.BookingItems
                .Where(item => item.ServiceItemID == serviceId)
                // Filter out Cancelled bookings here
                .AnyAsync(item => item.Booking!.EventDate.Date == eventDate.Date
                                  && item.Booking.BookingStatus != "Cancelled");
        }

       
        // 2. UPDATE: Added check for Cancelled Status
      
        public async Task<int> GetBookingCountForServiceOnDateAsync(Guid serviceId, DateTime eventDate)
        {
            return await _context.BookingItems
                .Where(item => item.ServiceItemID == serviceId)
                // Filter out Cancelled bookings here as well
                .Where(item => item.Booking!.EventDate.Date == eventDate.Date
                               && item.Booking.BookingStatus != "Cancelled")
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

        //  <--- NEW: Implementation for Tracking Logic
        public async Task<BookingItem?> GetBookingItemByIdAsync(Guid bookingItemId)
        {
            return await _context.BookingItems
                .Include(bi => bi.Booking)              // Loads Booking
                    .ThenInclude(b => b.Customer)       // ADD THIS: Loads Customer (so we get the Email)
                .Include(bi => bi.Service)
                .FirstOrDefaultAsync(bi => bi.BookingItemID == bookingItemId);
        }

        //  <--- NEW: Implementation to save tracking status
        public async Task UpdateBookingItemAsync(BookingItem item)
        {
            _context.BookingItems.Update(item);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsPackageBookedAsync(Guid packageId)
        {
            // Check if any BookingItem refers to this PackageID 
            // AND the booking is not Cancelled (If it was cancelled, we might allow deletion, 
            // but usually, we keep history. Let's start strict: If it appears in DB, don't delete).

            return await _context.BookingItems
                .AnyAsync(bi => bi.PackageID == packageId);
        }
    }
}

