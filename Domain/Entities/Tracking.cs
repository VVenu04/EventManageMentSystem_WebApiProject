using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Tracking
    {
        [Key]
        public Guid TrackID { get; set; }
        public Guid BookingID { get; set; }
        public string Status { get; set; } = string.Empty;

        // Navigation
        public Booking Booking { get; set; }
    }
}
