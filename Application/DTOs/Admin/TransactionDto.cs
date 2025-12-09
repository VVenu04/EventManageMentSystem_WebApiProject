using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Admin
{
    public class TransactionDto
    {
        public Guid PaymentID { get; set; }
        public string TransactionID { get; set; } // Stripe PaymentIntentId
        public Guid BookingID { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal AdminCommission { get; set; }
        public decimal VendorEarnings { get; set; }

        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
