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
        
        public async Task<ResponseModel<bool>> CreateOrder (int UserId,int branchId)
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

            decimal totalPrice = 0;

            foreach(var item in items)
            {
                if (item.Product.Quantity < item.Quantity)
                {
                    return new($"\"{item.Product.Name}\" mahsulotidan yetarli miqdorda qolmagan. Omborda: {item.Product.Quantity} ta", HttpStatusCode.BadRequest);
                }

                totalPrice += item.Product.Price * item.Quantity;
                item.Product.Quantity -= item.Quantity;
                
                await _productRepository.UpdateAsync(item.Product);
            }

            var branch = await _branchRepository.GetByIdAsync(branchId);

            if (branch == null)
                return new($"Branch topilmadi", HttpStatusCode.BadRequest);

            var order = new Order
            {
                CompanyBranchId = branchId,
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

            //var notificationRequest = new NotificationRequest(
            //    Type: NotificationType.OrderCreated,
            //    Title: "Yangi buyurtma",
            //    Body: $"Siz yangi buyurtma berdingiz. Buyurtma ID: {order.Id}",
            //    EntityType: NotificationEntityType.Order,
            //    EntityId: order.Id,
            //    BranchId: branchId,
            //    Priority: NotificationPriority.High
            //);
            await  _notificationservice.SendAsync(UserId,NotificationFactory.Created(order.Id, branchId));
            await _notificationRepository.SaveChangesAsync();
            //await _notificationservice.SendAsync(order.UserId, notificationRequest);
            //await _notificationRepository.SaveChangesAsync();

            //var notificationRequestToBranchAdmin = new NotificationRequest(
            //    Type: NotificationType.OrderCreated,
            //    Title: "Yangi buyurtma",
            //    Body: $"Sizda yangi buyurtma mavjud. Buyurtma Id:{order.Id}, UserId: {order.UserId}",
            //    EntityType: NotificationEntityType.Order,
            //    EntityId: order.Id,
            //    BranchId: branchId,
            //    Priority: NotificationPriority.High
            //    );

            var branchAdmins = await _userRepo.GetAsQueryable()
               .Where(b => b.Role == Role.SuperAdmin || b.CompanyBranchId == branchId)
               .Select(u => u.Id)
                .ToListAsync();



            await _notificationservice.SendToUsersAsync(branchAdmins, NotificationFactory.NewForBranch(order.Id, branchId));
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

        public async Task<ResponseModel<bool>> DeleteOrder (int id)
        {
             var deleted =  await _orderRepository.DeleteAsync(id);

                if (deleted == false)
                return new($"bu Order topilmadi", HttpStatusCode.NotFound);

             return new(true);
        }

        public async Task<ResponseModel<List<OrderDto>>> GetByOrderUserId (int UserId)
        {
            var orders =  await _orderRepository.GetAsQueryable()
                .Include(c => c.OrderItems)
                .ThenInclude(d => d.Product)
                .Where(c => c.UserId == UserId)
                .OrderByDescending(t=>t.CreatedAt)
                .ToListAsync();

            if (orders == null || !orders.Any())
                return new($"Bu mijozga tegishli buyurtmalar topilmadi", HttpStatusCode.NotFound);

            return new(_mapper.Map<List<OrderDto>>(orders));
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



