using Application.Extentions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public record NotificationRequest(
    NotificationType Type,
    string Title,
    string Body,
    NotificationEntityType? EntityType = null,
    long? EntityId = null,
    int? BranchId = null,
    NotificationPriority Priority = NotificationPriority.Normal,
    string? Link = null);

    public record NotificationDto(
    int Id, NotificationType Type, string Title, string Body, string? Link,
    NotificationEntityType? RelatedEntityType, long? RelatedEntityId,
    NotificationPriority Priority, bool IsRead, DateTime CreatedAt);
    public interface INotificationService
    {
        Task<ResponseModel<bool>> SendAsync(int userId, NotificationRequest request);
        Task<ResponseModel<bool>> SendToUsersAsync(List<int> userIds, NotificationRequest request);

        Task<ResponseModel<List<NotificationDto>>> GetMyAsync(int userId, int page, int size);
        Task<ResponseModel<int>> GetUnreadCountAsync(int userId);
        Task<ResponseModel<bool>> MarkReadAsync(int id, int userId);
        Task<ResponseModel<bool>> MarkAllReadAsync(int userId);
    }
}
