using Application.DTOs.Payment;
using Application.Extentions;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create")]
        public async Task<ResponseModel<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            return await _paymentService.CreatePaymentAsync(dto);
        }

        [HttpPost("confirm/{paymentId}")]
        public async Task<ResponseModel<PaymentDto>> ConfirmPayment(int paymentId,int adminId)
        {
            return await _paymentService.ConfirmPaymentAsync(paymentId, adminId);
        }

        [HttpGet("get-by-order/{orderId}")]
        public async Task<ResponseModel<PaymentDto>> GetPaymentByOrderId(int orderId)
        {
            return await _paymentService.GetPaymentByOrderIdAsync(orderId);
        }
        [Authorize(Roles ="SuperAdmin,CompanyAdmin,BranchAdmin")]
        [HttpGet("get-all")] 
        public async Task<ResponseModel<List<PaymentDto>>> GetAllPayments([FromQuery] PaymentStatus? status, [FromQuery] int? branchId)
        {
            return await _paymentService.GetAllPaymentsAsync(status, branchId);
        }

        [HttpPost("reject")]
        public async Task<ResponseModel<PaymentDto>> RejectPayment(RejectPaymentDto dto, int adminId)
        {
            return await _paymentService.RejectPaymentAsync( dto, adminId);
        }

        [HttpPost("upload-receipt")]
        public async Task<ResponseModel<string>> UploadReceipt(IFormFile file)
        {
            return await _paymentService.UploadReceiptAsync(file);
        }
    }
}
