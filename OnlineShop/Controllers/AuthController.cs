using Application.DTOs.Auth;
using Application.Extentions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ResponseModel<string>> Register(RegisterDto dto)
        {
             return  await _authService.RegisterAsync(dto);
        }

        [HttpPost("ConfirmEmail")]
        public async Task<ResponseModel<AuthResponseDto>> ConfirmEmail(ConfirmEmailDto dto)
        {
             return await _authService.ConfirmEmailAsync(dto);
        }

        [HttpPost("Login")]
        public async Task<ResponseModel<AuthResponseDto>> Login(LoginDto dto)
        {
             return await _authService.LoginAsync(dto);
        }

        [HttpPost("ForgotPassword")]
        public async Task<ResponseModel<string>> ForgotPassword(ForgotPasswordDto dto)
        {
            return await _authService.ForgotPasswordAsync(dto);
        }

        [HttpPost("ResetPassword")]
        public async Task<ResponseModel<string>> ResetPassword(ResetPasswordDto dto)
        {
            return await _authService.ResetPasswordAsync(dto);
        }
    }
}
