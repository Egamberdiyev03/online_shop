using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.Bot
{
    public class BotUpdateHandler : IUpdateHandler
    {
        private const string LoginButtonText = "🔑 Saytga kirish";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BotUpdateHandler> _logger;
        private readonly string _websiteUrl;

        public BotUpdateHandler(IServiceProvider serviceProvider, ILogger<BotUpdateHandler> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _websiteUrl = configuration["TelegramBot:WebsiteUrl"]?.TrimEnd('/') ?? "https://xalqbozor.uz";
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message || update.Message is null)
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;

            try
            {
                // Kontakt kelsa -> ro'yxatdan o'tkazamiz
                if (message.Contact is not null)
                {
                    await HandleContactAsync(botClient, message, cancellationToken);
                    return;
                }

                // /start, "Saytga kirish" tugmasi yoki har qanday boshqa xabar -> login havolasi
                await HandleLoginAsync(botClient, message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Botda xatolik yuz berdi. ChatId: {ChatId}", chatId);
                try
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Kechirasiz, vaqtinchalik xatolik yuz berdi. Qaytadan urinib ko'ring.",
                        cancellationToken: cancellationToken);
                }
                catch { /* foydalanuvchiga yetkazib bo'lmadi */ }
            }
        }

        // /start va boshqa xabarlar: mavjud foydalanuvchiga havola, yangiga telefon so'rash
        private async Task HandleLoginAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;

            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            var checkDto = new TelegramAuthDto
            {
                TelegramChatId = chatId,
                PhoneNumber = string.Empty,
                Name = message.From?.FirstName ?? message.Chat.FirstName ?? "Mijoz",
                TelegramUsername = message.Chat.Username
            };

            var authResult = await authService.TelegramAuthAsync(checkDto);

            if ((int)authResult.StatusCode is >= 200 and < 300 && authResult.result != null)
            {
                await SendLoginLinkAsync(botClient, authService, chatId, authResult.result.UserId,
                    authResult.result.Name, isNewUser: false, cancellationToken);
            }
            else
            {
                // Yangi foydalanuvchi -> telefon raqamini so'raymiz
                var contactButton = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("📱 Telefon raqamni ulashish") { RequestContact = true }
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                };

                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Assalomu alaykum, <b>{Encode(message.From?.FirstName ?? message.Chat.FirstName)}</b>!\n\n" +
                          "<b>XalqBozor</b> internet do'koniga xush kelibsiz! 😊\n\n" +
                          "Davom etish uchun pastdagi <b>«📱 Telefon raqamni ulashish»</b> tugmasini bosing:",
                    parseMode: ParseMode.Html,
                    replyMarkup: contactButton,
                    cancellationToken: cancellationToken);
            }
        }

        // Kontakt kelganda: ro'yxatdan o'tkazish + havola
        private async Task HandleContactAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var contact = message.Contact!;
            var chatId = message.Chat.Id;

            // Faqat o'z raqamini yuborishi mumkin
            if (contact.UserId != message.From?.Id)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Iltimos, faqat o'zingizning raqamingizni yuboring (pastdagi tugma orqali).",
                    cancellationToken: cancellationToken);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            var authDto = new TelegramAuthDto
            {
                TelegramChatId = chatId,
                PhoneNumber = contact.PhoneNumber.StartsWith("+") ? contact.PhoneNumber : $"+{contact.PhoneNumber}",
                Name = $"{contact.FirstName} {contact.LastName}".Trim(),
                TelegramUsername = message.Chat.Username
            };

            var response = await authService.TelegramAuthAsync(authDto);

            if ((int)response.StatusCode is >= 200 and < 300 && response.result != null)
            {
                await SendLoginLinkAsync(botClient, authService, chatId, response.result.UserId,
                    response.result.Name, isNewUser: true, cancellationToken);
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Xatolik yuz berdi: {response.Message ?? "Noma'lum xato"}",
                    cancellationToken: cancellationToken);
            }
        }

        // Backend kod yaratadi (hashi bazaga yoziladi), bot havolani yuboradi
        private async Task SendLoginLinkAsync(
            ITelegramBotClient botClient, IAuthService authService,
            long chatId, int userId, string name, bool isNewUser, CancellationToken cancellationToken)
        {
            var code = await authService.CreateTelegramLoginCodeAsync(userId);
            var loginUrl = $"{_websiteUrl}/login?code={code}";

            var inlineKeyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl("🚀 Saytga kirish", loginUrl));

            var greeting = isNewUser
                ? $"Tabriklaymiz, <b>{Encode(name)}</b>! 🎉 Siz muvaffaqiyatli ro'yxatdan o'tdingiz."
                : $"Assalomu alaykum, <b>{Encode(name)}</b>! 😊";

            await botClient.SendMessage(
                chatId: chatId,
                text: $"{greeting}\n\n" +
                      "Quyidagi tugmani bosing va <b>XalqBozor</b> saytiga parolsiz kiring.\n" +
                      "⏱ Havola <b>2 daqiqa</b> amal qiladi va bir marta ishlaydi.",
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);

            // Havola eskirsa, yangisini olish uchun pastda doimiy tugma
            await botClient.SendMessage(
                chatId: chatId,
                text: "Yangi havola kerak bo'lsa, pastdagi tugmani bosing 👇",
                replyMarkup: new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton(LoginButtonText) } })
                {
                    ResizeKeyboard = true
                },
                cancellationToken: cancellationToken);
        }

        private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Telegram Polling xatosi. Manba: {Source}", source);
            return Task.CompletedTask;
        }
    }
}
