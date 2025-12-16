using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class BookingItem
    {
        [Key]
        public Guid BookingItemID { get; set; }

        public Guid BookingID { get; set; }
        public Guid? ServiceItemID { get; set; } 
        public Guid? PackageID { get; set; }
        public Guid VendorID { get; set; }
        public decimal ItemPrice { get; set; }
        
        public string TrackingStatus { get; set; } = "Pending"; // Pending, Accepted, Preparing, OnTheWay, InProgress, Completed

        // CHANGE THIS LINE: Make it nullable 'string?'
        public string? CompletionOtp { get; set; }

        public DateTime? OtpExpiry { get; set; }

        public DateTime? ServiceDate { get; set; }

        // Navigation Properties
        public Booking? Booking { get; set; }

        public ServiceItem? Service { get; set; }

        public Package? Package { get; set; }
        public Vendor? Vendor { get; set; }
    }
}