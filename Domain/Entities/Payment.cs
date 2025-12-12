using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Payment
    {
        [Key]
        public Guid PaymentID { get; set; }
        public Guid BookingID { get; set; }

        public string StripePaymentIntentId { get; set; } // Stripe Transaction ID
        public decimal AmountPaid { get; set; } 
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } // "Pending", "Succeeded", "Failed"
        public string PaymentMethod { get; set; } // "cash back elad Cash
        public decimal AdminCommission { get; set; } 
        public decimal VendorEarnings { get; set; }  
        public decimal CustomerCashback { get; set; } 

        // Navigation
        public Booking Booking { get; set; }
    }
}
