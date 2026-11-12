using Application.DTOs.Order;
using Application.Extentions;
using Domain.Enums;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderService 
    {
        Task<ResponseModel<bool>> CreateOrder(int UserId);
        Task<ResponseModel<PagedResult<OrderDto>>> GetAllOrder(PagedQuery filter);
        Task<ResponseModel<OrderDto>> GetOrderById(int id);

        Task<ResponseModel<PagedResult<OrderDto>>> GetByOrderUserId(int userId, PagedQuery filter);
        Task<ResponseModel<PagedResult<OrderDto>>> GetOrdersByBranchId(int branchId, PagedQuery filter);
        Task<ResponseModel<PagedResult<OrderDto>>> GetOrdersByDateRange(DateTime from, DateTime to,PagedQuery filter);
        Task<ResponseModel<bool>> UpdateOrderStatus(int orderId, OrderStatus status);
        Task<ResponseModel<bool>> CancelOrder(int orderId);
    }
}
