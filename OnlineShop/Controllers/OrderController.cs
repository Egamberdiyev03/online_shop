using Application.DTOs.Order;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace OnlineShop.Controllers
{
    public class OrderController : BaseController 
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Token'dan BranchId olish uchun yordamchi metod
        private int? GetCurrentUserBranchId()
        {
            var branchIdClaim = User.FindFirst("companyBranchId")?.Value;
            if (int.TryParse(branchIdClaim, out int branchId))
                return branchId;
            return null;
        }

        // Tokendan rolni olish
        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
        [Authorize]
        [HttpPost("Create")]
        public async Task<ResponseModel<bool>> CreateOrder ()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(value, out var userId))
                return new("Foydalanuvchi aniqlanmadi", HttpStatusCode.Unauthorized);

            return await _orderService.CreateOrder(userId);
        }
            
        [HttpGet("GetById")]
        public async Task<ResponseModel<OrderDto>> GetOrderById (int id)
            => await _orderService.GetOrderById(id);
        
        // BranchManager ham ko'ra oladi, lekin faqat o'z filiali buyurtmalarini
        [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
        [HttpGet("GetAll")]
        public async Task<ResponseModel<PagedResult<OrderDto>>> GetAllOrder([FromQuery] PagedQuery filter)
        {
            var role = GetCurrentUserRole();

            // BranchManager uchun avtomatik filtr
            if (role == "BranchManager")
            {
                var userBranchId = GetCurrentUserBranchId();
                if (userBranchId.HasValue)
                {
                    return await _orderService.GetOrdersByBranchId(userBranchId.Value, filter);
                }
                return new("Filial topilmadi", HttpStatusCode.NotFound);
            }

            return await _orderService.GetAllOrder(filter);
        }

        [Authorize]
        [HttpGet("GetOrderByUserId")]
        public  async Task<ResponseModel<PagedResult<OrderDto>>> GetOrderByUserId([FromQuery] PagedQuery filter)
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(value, out var userId))
                return new("Foydalanuvchi aniqlanmadi", HttpStatusCode.Unauthorized);

            return  await  _orderService.GetByOrderUserId(userId, filter);
        }

        // BranchManager ham o'z filiali buyurtmalarini yangilay oladi
        [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
        [HttpPut("UpdateStatus")]
        public async Task<ResponseModel<bool>> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var role = GetCurrentUserRole();

            // BranchManager faqat o'z filiali buyurtmasini yangilay oladi
            if (role == "BranchManager")
            {
                var userBranchId = GetCurrentUserBranchId();
                var orderResult = await _orderService.GetOrderById(orderId);
                if (orderResult.result==null)
                {
                    return new("Buyurtma topilmadi", HttpStatusCode.NotFound);
                }

                if (!userBranchId.HasValue || orderResult.result.CompanyBranchId != userBranchId.Value)
                {
                    return new ResponseModel<bool>(
                        "Siz faqat o'z filialingiz buyurtmalarini yangilay olasiz.",
                        HttpStatusCode.Forbidden);
                }
            }

            return await _orderService.UpdateOrderStatus(orderId, status);
        }

        [Authorize]
        [HttpPut("Cancel")]
        public async Task<ResponseModel<bool>> CancelOrder(int orderId)
            => await _orderService.CancelOrder(orderId);
        
        // BranchManager ham chaqira oladi, lekin faqat o'z filiali uchun
        [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
        [HttpGet("GetOrdersByBranchId")]
        public async Task<ResponseModel<PagedResult<OrderDto>>> GetOrdersByBranchId(int branchId, [FromQuery] PagedQuery filter)
        {
            var role = GetCurrentUserRole();

            // BranchManager faqat o'z filiali buyurtmalarini ko'ra oladi
            if (role == "BranchManager")
            {
                var userBranchId = GetCurrentUserBranchId();
                if (userBranchId != branchId)
                {
                    return new("Siz faqat o'z filialingiz buyurtmalarini ko'ra olasiz.", HttpStatusCode.Forbidden);
                }
            }

            return await _orderService.GetOrdersByBranchId(branchId, filter);
        }

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpGet("GetOrdersByDateRange")]
        public async Task<ResponseModel<PagedResult<OrderDto>>> GetOrdersByDateRange(DateTime from, DateTime to, [FromQuery] PagedQuery filter)
            => await _orderService.GetOrdersByDateRange(from, to, filter);
    }
}
