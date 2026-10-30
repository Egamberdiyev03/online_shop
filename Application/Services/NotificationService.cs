using Application.Extentions;
using Application.Hubs;
using Application.Interfaces;
using DataAccess.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hub;
        public NotificationService(INotificationRepository repo,IHubContext<NotificationHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }

        public async Task<ResponseModel<List<NotificationDto>>> GetMyAsync(int userId, int page, int size)
        {
            var items = await _repo.GetUserNotificationsAsync(userId, page, size);
            return new(items.Select(n => new NotificationDto(
                n.Id, n.Type, n.Title, n.Body, n.Link,
                n.RelatedEntityType, n.RelatedEntityId,
                n.Priority, n.IsRead, n.CreatedAt)).ToList());
        }

        public async Task<ResponseModel<int>> GetUnreadCountAsync(int userId)
        {
            var count = await _repo.GetUnreadCountAsync(userId);
            return new(count);
        }

        public async Task<ResponseModel<bool>> MarkAllReadAsync(int userId)
        {
            await _repo.MarkAllAsReadAsync(userId);
            return new(true);
        }

        public async Task<ResponseModel<bool>> MarkReadAsync(int id, int userId)
        {
            var n = await _repo.GetByIdAsync(id, userId);
            if (n == null) return new(false);

            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow.AddHours(5);
            await _repo.SaveChangesAsync();
            return new(true);
        }

        public async Task<ResponseModel<bool>> SendAsync(int userId, NotificationRequest request)
        {
            return await SendToUsersAsync(new List<int> { userId }, request);
        }

        public async Task<ResponseModel<bool>> SendToUsersAsync(List<int> userIds, NotificationRequest request)
        {
            var items = userIds.Distinct().Select(id => new Notification
            {
                UserId = id,
                Type = request.Type,
                Title = request.Title,
                Body = request.Body,
                RelatedEntityType = request.EntityType,
                RelatedEntityId = request.EntityId,
                BranchId = request.BranchId,
                Priority = request.Priority,
                Link = request.Link
            }).ToList();

            if (items.Count == 0)
                return new ResponseModel<bool>("Qabul qiluvchi yo'q", HttpStatusCode.BadRequest, false);

            await _repo.AddRangeAsync(items);
            await _repo.SaveChangesAsync();

            foreach (var n in items)
            {
                await _hub.Clients.User(n.UserId.ToString()).SendAsync("ReceiveNotification", new
                {
                    n.Id,
                    n.Type,
                    n.Title,
                    n.Body,
                    n.Link,
                    n.RelatedEntityType,
                    n.RelatedEntityId,
                    n.Priority,
                    n.CreatedAt,
                    n.IsRead
                });
            }
            return new ResponseModel<bool>("OK", HttpStatusCode.OK, true);
        }

        
    }
}
