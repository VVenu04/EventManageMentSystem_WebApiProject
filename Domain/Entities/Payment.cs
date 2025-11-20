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
        public decimal AmountPaid { get; set; } // Customer கட்டிய மொத்த பணம்
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } // "Pending", "Succeeded", "Failed"

        // --- கணக்கு விவரங்கள் (Accounting) ---
        public decimal AdminCommission { get; set; } // Admin-க்கு வருவது (10% or 5%)
        public decimal VendorEarnings { get; set; }  // Vendor-க்குச் சேர்வது (90%)
        public decimal CustomerCashback { get; set; } // Wallet-ல் ஏறியது (5% or 0)

        // Navigation
        public Booking Booking { get; set; }
    }
}
