using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class PaymentRequestDto
    {
        public Guid PaymentID { get; set; }
        public Guid BookingID { get; set; }
        public string StripePaymentIntentId { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }

        public decimal CustomerCashback { get; set; }
    }
}
