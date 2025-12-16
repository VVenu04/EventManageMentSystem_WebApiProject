using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Notification
    {
        [Key]
        
        public Guid NotificationID { get; set; }

        public Guid UserID { get; set; } 
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info"; // "Booking", "Payment", "Alert"

        public Guid? RelatedEntityID { get; set; } // (BookingID or PackageID)

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
