using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Chat
{
    public class SendMessageDto
    {
        public Guid ReceiverID { get; set; }
        public string Content { get; set; }
        public Guid? BookingID { get; set; }
    }

    public class ChatHistoryDto
    {
        public Guid MessageID { get; set; }
        public Guid SenderID { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsMyMessage { get; set; } // Frontend-ல் வலது/இடது பக்கம் காட்ட
    }
}
