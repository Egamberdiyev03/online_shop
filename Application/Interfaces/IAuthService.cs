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
    }
}
 