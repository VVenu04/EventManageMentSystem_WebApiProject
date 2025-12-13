using Domain.Entities;
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(Payment payment);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<Payment?> GetByBookingIdAsync(Guid bookingId);

        // Payment Status-ஐ (Refunded என) மாற்ற
        Task UpdateAsync(Payment payment);
        Task<IEnumerable<Payment>> GetAllPaymentsWithDetailsAsync();
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId);
    }
}
