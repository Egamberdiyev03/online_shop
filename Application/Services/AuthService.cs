using Application.DTOs.Auth;
using Application.Extentions;
using Application.Interfaces;
using DataAccess.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Telegram.Bot.Types;
using User = Domain.Entities.User;

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
                CreatedAt = DateTime.UtcNow.AddHours(5),
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

            if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow.AddHours(5))
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

        public async Task<ResponseModel<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = _userRepo.GetAsQueryable()
                .FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
                return new("Ushbu email bilan ro'yxatdan o'tgan foydalanuvchi topilmadi.", HttpStatusCode.NotFound);

            var code = GenerateOtpCode();
            user.EmailConfirmationCode = code;
            user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userRepo.SaveChangesAsync();

            await _emailService.SendOtpCodeAsync(user.Email, code);

            return new("Tasdiqlash kodi emailingizga muvaffaqiyatli yuborildi.");
        }

        public async Task<ResponseModel<string>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = _userRepo.GetAsQueryable()
                .FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
                return new("Foydalanuvchi topilmadi.", HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(user.EmailConfirmationCode) || user.EmailConfirmationCode != dto.Code)
                return new("Tasdiqlash kodi noto'g'ri.", HttpStatusCode.Unauthorized);

            if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow.AddHours(5))
                return new("Kodning amal qilish muddati tugagan. Yangi kod so'rang.", HttpStatusCode.Unauthorized);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.EmailConfirmationCode = null;
            user.EmailConfirmationCodeExpiry = null;

            await _userRepo.SaveChangesAsync();

            return new("Parolingiz muvaffaqiyatli yangilandi. Yangi parol bilan tizimga kirishingiz mumkin.");
        }

        public async Task<ResponseModel<AuthResponseDto>> TelegramAuthAsync(TelegramAuthDto dto)
        {
            var user = await _userRepo.GetAsQueryable()
                .FirstOrDefaultAsync(u => u.TelegramChatId == dto.TelegramChatId);
             
            if(user == null)
            {
                if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    return new("Telefon raqam kerak", HttpStatusCode.NotFound);

                user = new User
                {
                    Name = dto.Name,
                    PhoneNumber = dto.PhoneNumber,
                    TelegramChatId = dto.TelegramChatId,
                    TelegramUsername = dto.TelegramUsername,
                    Role = Role.Customer,
                    CreatedAt = DateTime.UtcNow.AddHours(5),
                    IsTelegramConfirmed = true,
                    IsEmailConfirmed = false,
                    PasswordHash=string.Empty,
                    Email = string.Empty,
                    Address = string.Empty,
                    Location = string.Empty,

                };
                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();
            }
            // 3. Agar eski foydalanuvchi bo'lsa -> yangi telefon raqami yoki username bo'lsa yangilab qo'yamiz (Login)

            bool isUpdated = false;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && user.PhoneNumber != dto.PhoneNumber)
            {
                user.PhoneNumber = dto.PhoneNumber;
                isUpdated = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.TelegramUsername))
            {
                user.TelegramUsername = dto.TelegramUsername;
                isUpdated = true;
            }

            if (isUpdated)
                await _userRepo.SaveChangesAsync();
        
        // 4. JWT token generatsiya qilamiz va qaytaramiz
             var authResponse = _tokenService.GenerateToken(user);
             return new ("Muvaffaqiyatli", HttpStatusCode.OK, authResponse);
        }
    }
}
