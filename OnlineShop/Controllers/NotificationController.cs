using Application.Extentions;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        protected int UserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(value, out var id))
                    throw new UnauthorizedAccessException("Foydalanuvchi aniqlanmadi");
                return id;
            }
        }
   
        [HttpGet]
        public async Task<ResponseModel<List<NotificationDto>>> GetMyNotifications(int page = 1, int size = 10)
            => await _notificationService.GetMyAsync(UserId, page, size);

        [HttpGet("unread-count")]
        public async Task<ResponseModel<int>> GetUnreadCount()
            => await _notificationService.GetUnreadCountAsync(UserId);
    
        [HttpPost("mark-read/{id}")]
        public async Task<ResponseModel<bool>> MarkRead(int id)
            => await _notificationService.MarkReadAsync(id, UserId);

        [HttpPost("mark-all-read")]
        public async Task<ResponseModel<bool>> MarkAllRead()
            => await _notificationService.MarkAllReadAsync(UserId);

        [Authorize(Roles ="SuperAdmin")]
        [HttpPost("send")]
        public async Task<ResponseModel<bool>> Send(NotificationRequest request)
            => await _notificationService.SendAsync(UserId, request);
    }
}
