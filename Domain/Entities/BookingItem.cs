using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BookingItem
    {
        [Key]
        public Guid BookingItemID { get; set; }

        public Guid BookingID { get; set; }
        public Guid? ServiceID { get; set; }
        public Guid? PackageID { get; set; }
        public Guid VendorID { get; set; }
        public decimal ItemPrice { get; set; }
        public string TrackingStatus { get; set; }
        // Navigation
        public Booking? Booking { get; set; }
        public Service? Service { get; set; }
        public Package? Package { get; set; }
        public Vendor? Vendor { get; set; }
    }
}
