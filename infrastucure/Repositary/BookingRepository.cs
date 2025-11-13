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

        public async Task<Booking> GetByIdAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Customer) 
                .Include(b => b.BookingItems) 
                    .ThenInclude(item => item.Service) 
                        .ThenInclude(service => service.Vendor) 
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);
        }
        public async Task<bool> IsServiceBookedOnDateAsync(Guid serviceId, DateTime eventDate)
        {
            return await _context.BookingItems

                .Where(item => item.ServiceID == serviceId)

                .AnyAsync(item => item.Booking.EventDate.Date == eventDate.Date);
        }
        public async Task<int> GetBookingCountForVendorOnDateAsync(Guid vendorId, DateTime eventDate)
        {
            // BookingItems table-find
            var bookingIds = await _context.BookingItems

                .Where(item => item.VendorID == vendorId)

                .Where(item => item.Booking.EventDate.Date == eventDate.Date)

                .Select(item => item.BookingID)

                .Distinct()
                .ToListAsync();

            return bookingIds.Count;
        }
    }
}

