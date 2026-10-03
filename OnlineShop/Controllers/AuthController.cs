using Application.DTOs.Auth;
using Application.Extentions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
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
    }
}
