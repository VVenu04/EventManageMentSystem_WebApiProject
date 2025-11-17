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
    }
}

