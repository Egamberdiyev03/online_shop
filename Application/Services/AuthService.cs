using Application.DTOs.Auth;
using Application.Extentions;
using Application.Interfaces;
using DataAccess.Repositories;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthService(IRepository<User> userRepo, ITokenService tokenService, IEmailService emailService)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<ResponseModel<string>> RegisterAsync(RegisterDto dto)
        {
            var existing = _userRepo.GetAsQueryable()
                  .FirstOrDefault(u => u.Email == dto.Email);

            if (existing != null)
                return new("Bu email allaqachon ro'yxatdan o'tgan.",HttpStatusCode.Conflict);

            var code = GenerateOtpCode();

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Location = dto.Location,
                Role = Role.Customer,
                CreatedAt = DateTime.UtcNow,
                IsEmailConfirmed = false,
                EmailConfirmationCode = code,
                EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            await _emailService.SendOtpCodeAsync(user.Email, code);

            return new("Ro'yxatdan o'tish muvaffaqiyatli. Emailingizga yuborilgan kodni tasdiqlang.");
        }
        private static string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<ResponseModel<AuthResponseDto>> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = _userRepo.GetAsQueryable()
                 .FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
                return new("Foydalanuvchi topilmadi.",HttpStatusCode.NotFound);

            if (user.IsEmailConfirmed)
                 return new("Email allaqachon tasdiqlangan.",HttpStatusCode.OK);

            if (user.EmailConfirmationCode != dto.Code)
                return new("Kod noto'g'ri.",HttpStatusCode.Unauthorized);

            if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
                 return new("Kodning amal qilish muddati tugagan. Yangi kod so'rang.",HttpStatusCode.Unauthorized);

            user.IsEmailConfirmed = true;
            user.EmailConfirmationCode = null;
            user.EmailConfirmationCodeExpiry = null;

            await _userRepo.SaveChangesAsync();

            return new(_tokenService.GenerateToken(user));
        }

        public async Task<ResponseModel<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user =  _userRepo.GetAsQueryable()
                 .FirstOrDefault(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return new("Email yoki parol noto'g'ri.",HttpStatusCode.Unauthorized);

            if (!user.IsEmailConfirmed)
                return new("Email hali tasdiqlanmagan. Avval emailingizga kelgan kodni tasdiqlang.",HttpStatusCode.Forbidden);

            return new(_tokenService.GenerateToken(user));
        }


    }
}
