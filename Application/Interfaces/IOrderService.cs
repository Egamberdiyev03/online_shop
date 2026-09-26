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
        Task<ResponseModel<bool>> CreateOrder(int customerId, int branchId);
        Task<List<OrderDto>> GetAllOrder();
        Task<ResponseModel<OrderDto>> GetOrderById(int id);
        Task<ResponseModel<bool>> DeleteOrder(int id);


        Task<ResponseModel<List<OrderDto>>> GetByOrderCustomerId(int customerId);
        Task<ResponseModel<List<OrderDto>>> GetOrdersByBranchId(int branchId);
        Task<ResponseModel<List<OrderDto>>> GetOrdersByStatus(OrderStatus status);
        Task<ResponseModel<List<OrderDto>>> GetOrdersByDateRange(DateTime from, DateTime to);
        Task<ResponseModel<bool>> UpdateOrderStatus(int orderId, OrderStatus status);
        Task<ResponseModel<bool>> CancelOrder(int orderId);
    }
}
