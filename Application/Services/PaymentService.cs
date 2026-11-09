using Application.DTOs.Order;
using Application.DTOs.OrderItem;
using Application.DTOs.Payment;
using Application.Extentions;
using Application.Interfaces;
using AutoMapper;
using DataAccess.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _payRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<User> _userRepo;
        private readonly INotificationService _notifyService;
        private readonly IMapper _mapper;

        public PaymentService(
            IRepository<Payment> payRepo,
            IRepository<Order> orderRepo,
            IRepository<User> userRepo,
            IMapper mapper,
            INotificationService notifyService)
        {
            _payRepo = payRepo;
            _orderRepo = orderRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _notifyService = notifyService;
        }

        public async Task<ResponseModel<PaymentDto>> CreatePaymentAsync(CreatePaymentDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(dto.OrderId);

            if (order == null)
                return new("sizda faol buyurtma mavjud emas", HttpStatusCode.NotFound);

            if (order.UserId != dto.UserId)
                return new("Bu buyurtma sizning bo'yicha emas", HttpStatusCode.Forbidden);

            if (order.Status == OrderStatus.Cancelled)
                return new("Rad etilgan buyurtmaga tolov qilish mumkin emas", HttpStatusCode.BadRequest);

            if (order.Status == OrderStatus.Confirmed)
                return new("Buyurtma uchun tolov amalga oshirilgan", HttpStatusCode.BadRequest);

            if (dto.Amount <= 0)
                return new("To'lov summasi 0 dan katta bo'lishi kerak", HttpStatusCode.BadRequest);

            if (dto.Amount != order.TotalPrice)
                return new("To'lov summasi buyurtma summasiga teng bo'lishi kerak", HttpStatusCode.BadRequest);

            var payment = _mapper.Map<Payment>(dto);

            payment.CreatedAt = DateTime.UtcNow.AddHours(5);

            await _payRepo.AddAsync(payment);
            await _payRepo.SaveChangesAsync();

            // Adminlarga (SuperAdmin va shu filial mudirlariga) chek kelgani haqida xabar yuborish
            var adminUsers = await _userRepo.GetAsQueryable()
                .Where(u => u.Role == Role.SuperAdmin ||(u.Role==Role.BranchManager && u.CompanyBranchId == order.CompanyBranchId))
                .Select(u => u.Id)
                .ToListAsync();

            if (adminUsers.Any())
            {
                await _notifyService.SendToUsersAsync(adminUsers, NotificationFactory.PaymentSucceededForBranch(order.Id, order.CompanyBranchId));//new NotificationRequest(
                  /*  Type: NotificationType.PaymentSucceeded,
                    Title: "Yangi to'lov cheki keldi",
                    Body: $"#{order.Id} buyurtma uchun to'lov cheki yuborildi. Tasdiqlashingiz kutilmoqda.",
                    EntityType: NotificationEntityType.Order,
                    EntityId: order.Id,
                    BranchId: order.CompanyBranchId,
                    Priority: NotificationPriority.High,
                    Link: "/orders"
                ));*/
            }

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return new("To'lov muvaffaqiyatli amalga oshirildi", HttpStatusCode.OK, paymentDto);
        }

        public async Task<ResponseModel<PaymentDto>> ConfirmPaymentAsync(int paymentId, int adminId)
        {
            var payment = await _payRepo.GetAsQueryable()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return new("To'lov topilmadi", HttpStatusCode.NotFound);

            if (payment.Status != PaymentStatus.Pending)
                return new("Faqat kutilayotgan tolovni tasdiqlash mumkin", HttpStatusCode.BadRequest);

            if (payment.Status == PaymentStatus.Completed)
                return new("To'lov allaqachon tasdiqlangan", HttpStatusCode.BadRequest);

            if (payment.Status == PaymentStatus.Cancelled)
                return new("To`lov rad etilgan", HttpStatusCode.BadRequest);

            if (payment.Order != null && payment.Order.Status == OrderStatus.Cancelled)
                return new("Bu buyurtma bekor qilingan", HttpStatusCode.BadRequest);

            payment.Status = PaymentStatus.Completed;
            payment.ConfirmedByUserId = adminId;
            payment.ConfirmedAt = DateTime.UtcNow.AddHours(5);

            if (payment.Order != null)
            {
                payment.Order.Status = OrderStatus.Confirmed;
            }

            await _payRepo.SaveChangesAsync();

            if (payment.Order != null)
            {
                await _notifyService.SendAsync(payment.Order.UserId, NotificationFactory.PaymentSucceeded(payment.Order.Id, adminId));
            }

            return new("To'lov muvaffaqiyatli tasdiqlandi", HttpStatusCode.OK, _mapper.Map<PaymentDto>(payment));
        }

        public async Task<ResponseModel<List<PaymentDto>>> GetAllPaymentsAsync(PaymentStatus? status, int? branchId)
        {
            var payments = await _payRepo.GetAsQueryable()
                .AsNoTracking()
                .Include(p => p.Order)
                .Where(p => (!status.HasValue || p.Status == status) && (!branchId.HasValue || p.Order.CompanyBranchId == branchId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var paymentdtos = _mapper.Map<List<PaymentDto>>(payments);

            return new("To'lovlar muvaffaqiyatli olindi", HttpStatusCode.OK, paymentdtos);
        }

        public async Task<ResponseModel<PaymentDto>> GetPaymentByOrderIdAsync(int orderId, int userId = 0)
        {
            var query = _payRepo.GetAsQueryable()
                .AsNoTracking()
                .Include(p => p.Order)
                .Where(p => p.OrderId == orderId);

            if (userId > 0)
            {
                query = query.Where(p => p.Order.UserId == userId);
            }

            var payment = await query.FirstOrDefaultAsync();

            if (payment == null)
                return new("To'lov topilmadi", HttpStatusCode.NotFound);

            return new("To'lov muvaffaqiyatli olindi", HttpStatusCode.OK, _mapper.Map<PaymentDto>(payment));
        }

        public async Task<ResponseModel<PaymentDto>> RejectPaymentAsync(RejectPaymentDto dto, int adminId)
        {
            var payment = await _payRepo.GetAsQueryable()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(d => d.Id == dto.PaymentId);

            if (payment == null)
                return new("To'lov topilmadi", HttpStatusCode.NotFound);

            if (payment.Status != PaymentStatus.Pending)
                return new("Faqat kutilayotgan to'lovni rad etish mumkin", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return new("Rad etish sababini kiriting", HttpStatusCode.BadRequest);

            payment.Status = PaymentStatus.Failed;
            payment.ConfirmedByUserId = adminId;
            payment.RejectionReason = dto.Reason;
            payment.ConfirmedAt = DateTime.UtcNow.AddHours(5);

            if (payment.Order != null && payment.Order.Status == OrderStatus.Pending)
                payment.Order.Status = OrderStatus.Cancelled;

            await _payRepo.SaveChangesAsync();

            return new("To'lov rad etildi", HttpStatusCode.OK, _mapper.Map<PaymentDto>(payment));
        }

        public async Task<ResponseModel<string>> UploadReceiptAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new("Fayl topilmadi", HttpStatusCode.BadRequest);

            if (file.Length > 5 * 1024 * 1024)
                return new("Fayl hajmi 5MB dan oshmasligi kerak", HttpStatusCode.BadRequest);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return new("Faqat .jpg, .jpeg, .png, .pdf fayllar qabul qilinadi", HttpStatusCode.BadRequest);

            var receipsfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Receipts");

            if (!Directory.Exists(receipsfolder))
                Directory.CreateDirectory(receipsfolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            using (var stream = new FileStream(Path.Combine(receipsfolder, uniqueFileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/Receipts/{uniqueFileName}";

            return new("Fayl muvaffaqiyatli yuklandi", HttpStatusCode.OK, fileUrl);
        }

        public async Task<ResponseModel<PagedResult<PaymentDto>>> GetPaymentsByUserId(int userId, int pageNumber, int pageSize)
        {
            if(pageNumber < 1) pageNumber = 1;
            if(pageSize < 1) pageSize = 10;
            if(pageSize > 50) pageSize = 50;

            var query = _payRepo.GetAsQueryable()
                .AsNoTracking()
                .Where(p => p.Order.UserId == userId);

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(s=> new PaymentDto
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    Amount = s.Amount,
                    Status = s.Status,
                    ChekUrl = s.ChekUrl,
                    PaymentMethod = s.PaymentMethod,
                    TransactionNote = s.TransactionNote,
                    CreatedAt = s.CreatedAt,
                    ConfirmedAt = s.ConfirmedAt,
                    RejectionReason = s.RejectionReason,
                    ConfirmedByUserId = s.ConfirmedByUserId,
                    BranchName = s.Order.CompanyBranch.Name,
                    OrderCreatedAt = s.Order.CreatedAt
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paged = new PagedResult<PaymentDto>
            {
                Items = payments,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return new(paged);
        }
    }
}
