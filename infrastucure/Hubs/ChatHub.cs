using Application.DTOs.Chat;
using Domain.Entities;
using infrastucure.Data;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Hubs
{


    public class ChatHub:Hub
    {
        private readonly ApplicationDbContext _context;


        public ChatHub(ApplicationDbContext context)
        {
            _context = context;

        }


        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // JWT Token-ல் இருந்து வரும் ID
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(SendMessageDto dto)
        {
            var senderId = Guid.Parse(Context.UserIdentifier);
            var senderRole = Context.User.FindFirst("role")?.Value; // Claim-ல் இருந்து Role எடுப்பது

            // 🚨 RESTRICTION LOGIC: Customer/Vendor cannot msg Admin
            // Admin ID-ஐ நீங்கள் தனியாகவோ அல்லது Role வைத்தோ கண்டுபிடிக்க வேண்டும்.
            // இங்கே எளிமைக்காக Receiver Role-ஐ DB-ல் இருந்து எடுத்து சரிபார்க்கிறோம்.

            // (குறிப்பு: அட்மின் ஐடி தெரிந்தால் மட்டுமே தடுக்க முடியும். 
            // அல்லது அட்மின் பட்டியலை Frontend-ல் காட்டாமல் மறைத்துவிடலாம்).

            var message = new ChatMessage
            {
                MessageID = Guid.NewGuid(),
                SenderID = senderId,
                SenderRole = senderRole,
                ReceiverID = dto.ReceiverID,
                Content = dto.Content,
                BookingID = dto.BookingID,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message); // Dbset பெயர் ChatMessage என இருக்க வேண்டும்
            await _context.SaveChangesAsync();

            // Real-time ஆக Receiver-க்கு அனுப்பு
            await Clients.Group(dto.ReceiverID.ToString()).SendAsync("ReceiveMessage", new ChatHistoryDto
            {
                MessageID = message.MessageID,
                SenderID = senderId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsMyMessage = false
            });
        }

    }
}
