using Application.DTOs.Order;
using Application.DTOs.OrderItem;
using Application.Extentions;
using Application.Interfaces;
using AutoMapper;
using DataAccess.Database;
using DataAccess.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.AccessControl;
//using Telegram.Bot.Types;
namespace Application.Services
{
    public  class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<CompanyBranch> _branchRepository;
        private readonly INotificationService _notificationservice;
        private readonly INotificationRepository _notificationRepository;
        private readonly IRepository<User> _userRepo; 
        private readonly IMapper _mapper;
        public OrderService
            (IRepository<Order> orderRepository,
            IMapper mapper,
            IRepository<Cart> cartRepository,
            IRepository<CompanyBranch> branchRepository,
            IRepository<Product> productRepository,
            INotificationService notificationservice,
            INotificationRepository notificationRepository,
            IRepository<User> userRepo
            )
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _branchRepository = branchRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _notificationservice = notificationservice;
            _notificationRepository = notificationRepository;
            _userRepo = userRepo;
        }
        public async Task<ResponseModel<bool>> CreateOrder (int UserId)
        {            
            var cart = await _cartRepository
                .GetAsQueryable()
                .Include(c => c.CartItems)
                .ThenInclude(t=>t.Product)
                .FirstOrDefaultAsync(s => s.UserId == UserId);

            if (cart == null)
                return new($"Bu Userda faol savat topilmadi ", HttpStatusCode.NotFound);
            var items = cart.CartItems.ToList();

            if (!items.Any())
                return new("Savat bo`sh", HttpStatusCode.BadRequest);

            var orderBranchId = items.First().Product.CompanyBranchId;

            if (items.Any(i => i.Product.CompanyBranchId != orderBranchId))
                return new("Savatda turli filial mahsulotlari bor. Savatni tozalab, qayta qo'shing.", HttpStatusCode.BadRequest);

            foreach(var item in items)
            {
                if (item.Product.Quantity < item.Quantity)
                {
                    return new($"\"{item.Product.Name}\" mahsulotidan yetarli miqdorda qolmagan. Omborda: {item.Product.Quantity} ta", HttpStatusCode.BadRequest);
                }
            }

            decimal totalPrice = items.Sum(i => i.Product.Price * i.Quantity);

            foreach (var item in items)
            {
                item.Product.Quantity -= item.Quantity;
                await _productRepository.UpdateAsync(item.Product);
            }

            var order = new Order
            {
                CompanyBranchId = orderBranchId,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow.AddHours(5),
                UserId = cart.UserId,
                Status = OrderStatus.Pending,
                ProductCount = cart.CartItems.Count,
                OrderItems = cart.CartItems.Select(o => new OrderItem
                {
                    ProductId = o.ProductId,
                    Quantity = o.Quantity,
                    UnitPrice = o.Product.Price
                }).ToList()
            };

            await _orderRepository.AddAsync(order);
            await _cartRepository.DeleteAsync(cart.Id);
            await _cartRepository.SaveChangesAsync();
            await  _notificationservice.SendAsync(UserId,NotificationFactory.Created(order.Id, orderBranchId));
            await _notificationRepository.SaveChangesAsync();

            var branchAdmins = await _userRepo.GetAsQueryable()
               .Where(b => b.Role == Role.SuperAdmin || b.CompanyBranchId == orderBranchId)
               .Select(u => u.Id)
                .ToListAsync();

            await _notificationservice.SendToUsersAsync(branchAdmins, NotificationFactory.NewForBranch(order.Id, orderBranchId));
            await _notificationRepository.SaveChangesAsync();
            //await _notificationservice.SendToUsersAsync(branchAdmins, notificationRequestToBranchAdmin);
            return new(true);
        }
        public async Task<List<OrderDto>> GetAllOrder ()
        {
            var orders = await _orderRepository.GetAsQueryable().Select(c => new OrderDto
            {
                Id = c.Id,
                CompanyBranchId= c.CompanyBranchId,
                UserId = c.UserId,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                TotalPrice = c.TotalPrice,
                OrderItems = c.OrderItems.Select(o => new OrderItemDto
                {
                    ProductId = o.ProductId,
                    Quantity = o.Quantity,
                    ProductName = o.Product.Name,
                    UnitPrice = o.UnitPrice
                }).ToList()
            }).ToListAsync();

            return orders;
        }
        public async Task<ResponseModel<OrderDto>> GetOrderById(int id)
        {
           var order= await _orderRepository.GetAsQueryable()
                .Include(c=>c.OrderItems)
                .ThenInclude(c=>c.Product)
                .FirstOrDefaultAsync(d=>d.Id==id);

            if (order == null)
                return new($"Order topilmadi", HttpStatusCode.NotFound);

            return new(_mapper.Map<OrderDto>(order));
        }
        public async Task<ResponseModel<PagedResult<OrderDto>>> GetByOrderUserId (int UserId,int pageSize,int pageNumber)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _orderRepository.GetAsQueryable()
                 .Where(c => c.UserId == UserId);

            var totalCount = await query.CountAsync();

            var orders =  await query
                .AsNoTracking()
                .Include(c => c.OrderItems)
                .ThenInclude(d => d.Product)
                .OrderByDescending(t=>t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<OrderDto>>(orders);

            var paged = new PagedResult<OrderDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new("Muvaffaqiyatli", HttpStatusCode.OK, paged);
        }
        public async Task<ResponseModel<List<OrderDto>>> GetOrdersByBranchId(int branchId)
        {
            var orders = await _orderRepository.GetAsQueryable()
             .Include(c => c.OrderItems)
             .ThenInclude(d => d.Product)
             .Where(c => c.CompanyBranchId == branchId)
             .OrderByDescending(t => t.CreatedAt)
             .ToListAsync();

            if (orders == null || !orders.Any())
                return new($"Bu branchga tegishli buyurtmalar topilmadi", HttpStatusCode.NotFound);

            return new(_mapper.Map<List<OrderDto>>(orders));
        }
        public async Task<ResponseModel<List<OrderDto>>> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _orderRepository.GetAsQueryable()
              .Include(c => c.OrderItems)
              .ThenInclude(d => d.Product)
              .Where(c => c.Status==status)
              .OrderByDescending(t => t.CreatedAt)
              .ToListAsync();

            if (orders == null || !orders.Any())
                return new($"Bu statusga tegishli buyurtmalar topilmadi", HttpStatusCode.NotFound);

            return new(_mapper.Map<List<OrderDto>>(orders));
        }
        public async Task<ResponseModel<List<OrderDto>>> GetOrdersByDateRange(DateTime from, DateTime to)
        {
            var uzbekistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tashkent");
            from = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(from, DateTimeKind.Unspecified), uzbekistanTimeZone);
            to = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(to, DateTimeKind.Unspecified), uzbekistanTimeZone);

            var orders = await _orderRepository.GetAsQueryable()
                 .Include(c => c.OrderItems)
                 .ThenInclude(d => d.Product)
                 .Where(c => c.CreatedAt>=from && c.CreatedAt<=to)
                 .OrderByDescending(t => t.CreatedAt)
                 .ToListAsync();

            if (orders == null || !orders.Any())
                return new($"Bu vaqt oraligida buyurtmalar topilmadi", HttpStatusCode.NotFound);

            return new(_mapper.Map<List<OrderDto>>(orders));
        }
        public async Task<ResponseModel<bool>> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
                return new($"Bu OrderId {orderId} li buyurtma topilmadi", HttpStatusCode.NotFound);

            if (status == OrderStatus.Cancelled)
            {
                return await CancelOrder(orderId);
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                return new("Bekor qilingan buyurtma statusini o'zgartirib bo'lmaydi", HttpStatusCode.BadRequest);
            }

            if (order.Status == OrderStatus.Delivered)
            {
                return new("Yetkazilgan buyurtma allaqachon yakunlangan", HttpStatusCode.BadRequest);
            }

            order.Status = status;
            await _orderRepository.SaveChangesAsync();
            try
            {
                if (status == OrderStatus.Shipped)
                {
                    await _notificationservice.SendAsync(order.UserId, NotificationFactory.OnTheWay(order.Id, order.CompanyBranchId));
                }
                else if (status == OrderStatus.Delivered)
                {
                    await _notificationservice.SendAsync(order.UserId, NotificationFactory.Completed(order.Id, order.CompanyBranchId, false));
                }
            }
            catch
            {
                // Bildirishnoma yuborishdagi xatolik asosiy amalni to'xtatmaydi
            }
            return new(true);
        }
        public async Task<ResponseModel<bool>> CancelOrder(int orderId)
        {
            var order = await _orderRepository.GetAsQueryable()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return new($"Order topilmadi", HttpStatusCode.NotFound);

            if (order.Status == OrderStatus.Cancelled)
                return new("Order allaqachon bekor qilingan", HttpStatusCode.BadRequest);

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
                return new($"{order.Status} holatidagi orderni bekor qilib bo'lmaydi", HttpStatusCode.BadRequest);

            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                    product.Quantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.SaveChangesAsync();
            await _notificationservice.SendAsync(order.UserId, NotificationFactory.CancelledForCustomer(order.Id, order.CompanyBranchId));

            var branchAdmins = await _userRepo.GetAsQueryable()
               .Where(b => b.CompanyBranchId == order.CompanyBranchId)
               .Select(u => u.Id)
                .ToListAsync();

            await _notificationservice.SendToUsersAsync(branchAdmins, NotificationFactory.CancelledByCustomer(order.Id, order.CompanyBranchId));
            return new(true);
        }
    }
}
