using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface INotificationService
    {
        Task SendNotificationAsync(Guid userId, string message, string type, Guid? relatedId);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
    }
}
