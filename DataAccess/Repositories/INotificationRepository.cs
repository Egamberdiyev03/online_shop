using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public interface INotificationRepository
    {
        Task AddRangeAsync(List<Notification> items);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, int page, int size);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notification?> GetByIdAsync(int id, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task SaveChangesAsync();
    }
}
