using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingConfirmationDto
    {
        public Guid BookingID { get; set; }
        public Guid CustomerID { get; set; }
        public string CustomerName { get; set; }
        public DateTime EventDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string BookingStatus { get; set; }

        public List<BookingItemDto> BookedItems { get; set; }
    }

    public class BookingItemDto
    {
        public Guid BookingItemID { get; set; }
        public string ServiceName { get; set; }
        public decimal ItemPrice { get; set; }
        public string VendorName { get; set; }
        public string TrackingStatus { get; set; } // "Confirmed"
    }
}
