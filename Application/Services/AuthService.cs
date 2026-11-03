using Application.DTOs.Auth;
using Application.Extentions;
using Application.Interfaces;
using Application.Services.Bot;
using DataAccess.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Telegram.Bot.Types;
using User = Domain.Entities.User;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IRepository<User> _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IRepository<TelegramLoginCode> _codeRepo;

        public AuthService(IRepository<User> userRepo,
            ITokenService tokenService,
            IEmailService emailService,
            IRepository<TelegramLoginCode> codeRepo,
            IConfiguration config)
        
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _emailService = emailService;
            _codeRepo = codeRepo;
            _config = config;
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
             return new ("Muvaffaqiyatli", HttpStatusCode.OK, new AuthResponseDto
             {
                 UserId = user.Id,
                 Name = user.Name,
             });
        }

        public async Task<string> CreateTelegramLoginCodeAsync(int userId)
        {

            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

            await _codeRepo.AddAsync(new TelegramLoginCode
            {
                UserId = userId,
                CodeHash = hash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(2)
            });
            await _codeRepo.SaveChangesAsync();
            return code;
        }

        public async Task<ResponseModel<AuthResponseDto>> TelegramLoginByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length > 128)
                return new("Havola yaroqsiz", HttpStatusCode.Unauthorized);

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

            var entry = await _codeRepo.GetAsQueryable()
                .FirstOrDefaultAsync(c => c.CodeHash == hash && !c.Used && c.ExpiresAt > DateTime.UtcNow);

            if (entry == null)
                return new("Havola yaroqsiz yoki muddati tugagan", HttpStatusCode.Unauthorized);

            // Atomik: faqat bitta so'rov o'tadi
            var updated = await _codeRepo.GetAsQueryable()
                .Where(c => c.Id == entry.Id && !c.Used)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Used, true));

            if (updated == 0)
                return new("Havola yaroqsiz yoki muddati tugagan", HttpStatusCode.Unauthorized);

            var user = await _userRepo.GetAsQueryable().FirstOrDefaultAsync(u => u.Id == entry.UserId);
            if (user == null || user.Role != Role.Customer)
                return new("Bu akkaunt Telegram orqali kira olmaydi", HttpStatusCode.Forbidden);

            return new("Muvaffaqiyatli", HttpStatusCode.OK, _tokenService.GenerateToken(user));
        }

        public async Task<ResponseModel<AuthResponseDto>> TelegramMiniAppLoginAsync(string initData)
        {
            var botToken = _config["TelegramBot:BotToken"];   // <-- o'zingizdagi kalit nomiga moslang
            if (string.IsNullOrWhiteSpace(botToken))
                return new("Server sozlamasi xato", HttpStatusCode.InternalServerError);

            if (!TelegramInitData.TryValidate(initData, botToken, out var telegramId))
                return new("Telegram ma'lumoti yaroqsiz yoki muddati tugagan", HttpStatusCode.Unauthorized);

            var user = await _userRepo.GetAsQueryable()
                .FirstOrDefaultAsync(u => u.TelegramChatId == telegramId);

            if (user == null)
                return new("Avval Telegram botda ro'yxatdan o'ting", HttpStatusCode.NotFound);

            if (user.Role != Role.Customer)
                return new("Bu akkaunt Telegram orqali kira olmaydi", HttpStatusCode.Forbidden);

            return new("Muvaffaqiyatli", HttpStatusCode.OK, _tokenService.GenerateToken(user));
        }
    }
}
