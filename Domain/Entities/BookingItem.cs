using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class BookingItem
    {
        [Key]
        public Guid BookingItemID { get; set; }

        public Guid BookingID { get; set; }
        public Guid? ServiceItemID { get; set; } // ServiceID அல்ல
        public Guid? PackageID { get; set; }
        public Guid VendorID { get; set; }
        public decimal ItemPrice { get; set; }
        public string TrackingStatus { get; set; }

        // Navigation Properties
        public Booking? Booking { get; set; }

        // 🚨 இங்கே கவனிக்கவும்: நாம் Class பெயரை 'ServiceItem' என மாற்றினோம்.
        // ஆனால் Property பெயர் 'Service' ஆக இருந்தால் Mapper-ல் 'item.Service' எனப் பயன்படுத்தலாம்.
        public ServiceItem? Service { get; set; }

        public Package? Package { get; set; }
        public Vendor? Vendor { get; set; }
    }
}