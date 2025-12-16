using Application.Interface.IRepo;
using Application.Interface.IService;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Application.Services
{
    public class NotificationService:INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IRealTimeNotifier _realTimeNotifier; 

        public NotificationService(INotificationRepository notificationRepo,
                                   IRealTimeNotifier realTimeNotifier) 
        {
            _notificationRepo = notificationRepo;
            _realTimeNotifier = realTimeNotifier;
        }

        public async Task SendNotificationAsync(Guid userId, string message, string type, Guid? relatedId)
        {
            
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Message = message,
                Type = type,
                RelatedEntityID = relatedId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepo.AddAsync(notification);

            await _realTimeNotifier.SendToUserAsync(userId.ToString(), message);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _notificationRepo.GetByUserIdAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _notificationRepo.UpdateAsync(notification);
            }
        }
    }
}

