using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class UpdateTrackingDto
    {
        public Guid BookingItemID { get; set; }
        public string Status { get; set; } // "Preparing", "OnTheWay", "InProgress"
    }


}
