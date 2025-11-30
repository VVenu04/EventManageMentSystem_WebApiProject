using Application.Interface.IRepo;
using Domain.Entities;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class NotificationRepository: INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            _context.Set<Notification>().Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Set<Notification>()
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.CreatedAt) // புதியது மேலே வர வேண்டும்
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid notificationId)
        {
            return await _context.Set<Notification>().FindAsync(notificationId);
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Set<Notification>().Update(notification);
            await _context.SaveChangesAsync();
        }
    }
}
