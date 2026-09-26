using Application.DTOs.Order;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("Create")]
        public Task<ResponseModel<bool>> CreateOrder (int customerId, int branchId)
        {
            return   _orderService.CreateOrder(customerId,branchId);   
        }

        [HttpGet("GetById")]
        public async Task<ResponseModel<OrderDto>> GetOrderById (int id)
        {
            return await _orderService.GetOrderById(id);
        }

        [HttpGet("GetAll")]
        public async Task<ResponseModel<List<OrderDto>>> GetAllOrder()
        {
          return new( await  _orderService.GetAllOrder());
        }

        [HttpGet("GetOrderByCustomerId")]
        public async Task<ResponseModel<List<OrderDto>>> GetOrderByCustomerId(int customerId)
        {
            var orders =  await _orderService.GetByOrderCustomerId(customerId);

            return orders;
        }

        [HttpPut("UpdateStatus")]
        public async Task<ResponseModel<bool>> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            return await _orderService.UpdateOrderStatus(orderId, status);
        }

        [HttpPut("Cancel")]
        public async Task<ResponseModel<bool>> CancelOrder(int orderId)
        {
            return await _orderService.CancelOrder(orderId);
        }

        [HttpGet("GetOrdersByBranchId")]
        public async Task<ResponseModel<List<OrderDto>>> GetOrdersByBranchId(int branchId)
        {
            return await _orderService.GetOrdersByBranchId(branchId);
        }

        [HttpGet("GetOrdersByStatus")]
        public async Task<ResponseModel<List<OrderDto>>> GetOrdersByStatus(OrderStatus status)
        {
            return await _orderService.GetOrdersByStatus(status);
        }

        [HttpGet("GetOrdersByDateRange")]
        public async Task<ResponseModel<List<OrderDto>>> GetOrdersByDateRange(DateTime from, DateTime to)
        {
            return await _orderService.GetOrdersByDateRange(from, to);
        }
    }
}
