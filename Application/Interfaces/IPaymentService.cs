using Application.DTOs.Payment;
using Application.Extentions;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPaymentService
    {
        Task<ResponseModel<PaymentDto>> CreatePaymentAsync(CreatePaymentDto dto);

        Task<ResponseModel<PaymentDto>> ConfirmPaymentAsync(int paymentId, int adminId);

        Task<ResponseModel<PaymentDto>> GetPaymentByOrderIdAsync(int orderId);
        Task<ResponseModel<List<PaymentDto>>> GetAllPaymentsAsync(PaymentStatus? status, int? branchId);
        Task<ResponseModel<PaymentDto>> RejectPaymentAsync(RejectPaymentDto dto, int adminId);

        Task<ResponseModel<string>> UploadReceiptAsync(IFormFile file);
    }
}
