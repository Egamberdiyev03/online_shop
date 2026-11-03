using Application.DTOs.Auth;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public  interface IAuthService
    {
        Task<ResponseModel<string>> RegisterAsync(RegisterDto dto);
        Task<ResponseModel<AuthResponseDto>> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<ResponseModel<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<ResponseModel<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ResponseModel<string>> ResetPasswordAsync(ResetPasswordDto dto);
        Task<ResponseModel<AuthResponseDto>> TelegramAuthAsync(TelegramAuthDto dto);

        Task<string> CreateTelegramLoginCodeAsync(int userId);
        Task<ResponseModel<AuthResponseDto>> TelegramLoginByCodeAsync(string code);
        Task<ResponseModel<AuthResponseDto>> TelegramMiniAppLoginAsync(string initData);
    }
}
 