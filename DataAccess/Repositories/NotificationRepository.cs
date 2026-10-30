using DataAccess.Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddRangeAsync(List<Notification> items)
        => await _db.Notifications.AddRangeAsync(items);
        public async Task<Notification?> GetByIdAsync(int id, int userId)
        => await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        public async Task<int> GetUnreadCountAsync(int userId)
         => await _db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead);
        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int page, int size)
         => await _db.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync();

        public async Task MarkAllAsReadAsync(int userId)
        => await _db.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, DateTime.UtcNow.AddHours(5)));

        public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
