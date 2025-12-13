using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Payment;
using Domain.Entities;

namespace Application.Mapper
{
    public  static class PaymentMapper
    {
        public static PaymentRequestDto MapToDto(Payment payment)
        {
            if (payment == null) return null;

            return new PaymentRequestDto
            {
                PaymentID = payment.PaymentID,
                BookingID = payment.BookingID,
                TransactionId = payment.TransactionId,
                AmountPaid = payment.AmountPaid,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                CustomerCashback = payment.CustomerCashback
            };
        }
    }
}
