using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ChatMessage
    {
        [Key]
        public Guid MessageID { get; set; }

        public Guid SenderID { get; set; } 
        public string SenderRole { get; set; } // Customer / Vendor / Admin

        public Guid ReceiverID { get; set; } // யாருக்கு
        public string ReceiverRole { get; set; }

        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public Guid? BookingID { get; set; }
    }
}
