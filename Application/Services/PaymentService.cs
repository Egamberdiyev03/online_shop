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
        private readonly IMapper _mapper;

        public PaymentService(IRepository<Payment> payRepo, IRepository<Order> orderRepo, IMapper mapper)
        {
            _payRepo = payRepo;
            _orderRepo = orderRepo;
            _mapper = mapper;
        }


        public async Task<ResponseModel<PaymentDto>> CreatePaymentAsync(CreatePaymentDto dto)
        {
            var order = await _orderRepo.GetByIdAsync(dto.OrderId);

            if (order == null)
                return new("sizda faol buyurtma mavjud emas", HttpStatusCode.Forbidden);

            if (order.UserId != dto.UserId)
                return new("Bu buyurtma sizning bo'yicha emas", HttpStatusCode.Forbidden);

            if (order.Status == OrderStatus.Confirmed)
                return new("Buyurtma uchun tolov amalga oshirilgan", HttpStatusCode.BadRequest);

            if (dto.Amount <= 0)
                return new("To'lov summasi 0 dan katta bo'lishi kerak", HttpStatusCode.BadRequest);

            if (dto.Amount != order.TotalPrice)
                return new("To'lov summasi buyurtma summasiga teng bo'lishi kerak", HttpStatusCode.BadRequest);


            var payment = _mapper.Map<Payment>(dto);

            await _payRepo.AddAsync(payment);
            await _payRepo.SaveChangesAsync();

            var paymentDto = _mapper.Map<PaymentDto>(payment);
            return new("To'lov muvaffaqiyatli amalga oshirildi", HttpStatusCode.OK, paymentDto);
        }

        public async Task<ResponseModel<PaymentDto>> ConfirmPaymentAsync(int paymentId, int adminId)
        {
            var payment = await _payRepo.GetByIdAsync(paymentId);

            if (payment == null)
                return new("To'lov topilmadi", HttpStatusCode.NotFound);

            if (payment.Status == PaymentStatus.Completed)
                return new("To'lov allaqachon tasdiqlangan", HttpStatusCode.BadRequest);

            if (payment.Status == PaymentStatus.Cancelled)
                return new("To`lov rad etilgan", HttpStatusCode.BadRequest);

            payment.Status = PaymentStatus.Completed;
            payment.ConfirmedByUserId = adminId;
            payment.ConfirmedAt = DateTime.UtcNow;

            await _payRepo.UpdateAsync(payment);

            var order = await _orderRepo.GetByIdAsync(payment.OrderId);

            if (order.Status == OrderStatus.Cancelled)
                return new("Bu buyurtma rad etilgan", HttpStatusCode.BadRequest);

            order.Status = OrderStatus.Confirmed;
            await _orderRepo.UpdateAsync(order);
            await _orderRepo.SaveChangesAsync();

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

        public async Task<ResponseModel<PaymentDto>> GetPaymentByOrderIdAsync(int orderId)
        {
            var payment = await _payRepo.GetAsQueryable()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null)
                return new("To'lov topilmadi", HttpStatusCode.NotFound);

            return new("To'lov muvaffaqiyatli olindi", HttpStatusCode.OK, _mapper.Map<PaymentDto>(payment));
        }

        public async Task<ResponseModel<PaymentDto>> RejectPaymentAsync(RejectPaymentDto dto, int adminId)
        {
            var payment = await _payRepo.GetByIdAsync(dto.PaymentId);

            if (payment == null)
                return new("To'lov topilmadi", HttpStatusCode.NotFound);

            if (payment.Status == PaymentStatus.Completed)
                return new("Tasdiqlangan to'lovni rad etib bo'lmaydi", HttpStatusCode.BadRequest);

            payment.Status = PaymentStatus.Failed;
            payment.ConfirmedByUserId = adminId;
            payment.RejectionReason = dto.Reason;
            payment.ConfirmedAt = DateTime.UtcNow;

            await _payRepo.UpdateAsync(payment);
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
    }
} 