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
        Task<string> CreatePaymentIntentAsync(PaymentRequestDto dto);
        Task<bool> ConfirmPaymentAndDistributeFundsAsync(string paymentIntentId);
        Task<bool> RefundPaymentAsync(Guid bookingId);
        Task<IEnumerable<WalletTransactionDto>> GetCustomerWalletHistoryAsync(Guid customerId);
    }
}
