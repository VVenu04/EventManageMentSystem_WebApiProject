using Application.Interface.IRepo;
using Domain.Entities;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class PaymentRepository: IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);
        }

        // --- 🚨 FIX: '?' சேர்க்கவும் ---
        public async Task<Payment?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(p => p.BookingID == bookingId);
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Payment>> GetAllPaymentsWithDetailsAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Customer) // Customer விவரம் தேவை
                .OrderByDescending(p => p.PaymentDate) // புதியது முதலில்
                .ToListAsync();
        }
    }
}
