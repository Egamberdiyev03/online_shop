using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.OrderItem
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal  TotalPrice { get; set; }
        public OrderStatus Status { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }
}
