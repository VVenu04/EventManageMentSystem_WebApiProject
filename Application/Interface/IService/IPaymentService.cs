using Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IPaymentService
    {
        Task<bool> ProcessMockPaymentAsync(Guid bookingId);
        Task<bool> RefundPaymentAsync(Guid bookingId);
        Task<IEnumerable<WalletTransactionDto>> GetCustomerWalletHistoryAsync(Guid customerId);
    }
}
