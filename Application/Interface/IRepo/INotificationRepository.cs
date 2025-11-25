using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);

        // ஒரு User-இன் அனைத்து Notifications-ஐயும் பெற
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);

        // ஒரு குறிப்பிட்ட Notification-ஐப் பெற (Mark as Read-க்குத் தேவை)
        Task<Notification?> GetByIdAsync(Guid notificationId);

        // Notification-ஐ Update செய்ய (Mark as Read)
        Task UpdateAsync(Notification notification);
    }
}
