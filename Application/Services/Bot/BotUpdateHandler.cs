using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BotUpdateHandler> _logger;
        private readonly string _websiteUrl;

        public BotUpdateHandler(IServiceProvider serviceProvider, ILogger<BotUpdateHandler> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            // appsettings.json dan "WebsiteUrl" olinadi, topilmasa "https://xalqbozor.uz" ishlatiladi
            _websiteUrl = configuration["TelegramBot:WebsiteUrl"]?.TrimEnd('/') ?? "https://xalqbozor.uz";
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message || update.Message is null)
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim() ?? string.Empty;

            try
            {
                // 1. /start yoki /start auth
                if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleStartAsync(botClient, message, cancellationToken);
                    return;
                }

                // 2. Kontakt (Telefon raqam) kelganda
                if (message.Contact is not null)
                {
                    await HandleContactAsync(botClient, message, cancellationToken);
                    return;
                }

                // 3. Pastdagi doimiy menyu tugmalari:
                if (text.Contains("Mahsulotlar", StringComparison.OrdinalIgnoreCase) || text.Contains("katalog", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleCatalogAsync(botClient, chatId, cancellationToken);
                    return;
                }

                if (text.Contains("Savatcha", StringComparison.OrdinalIgnoreCase) || text.Contains("Savat", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleCartAsync(botClient, chatId, cancellationToken);
                    return;
                }

                if (text.Contains("Buyurtma", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleOrdersAsync(botClient, chatId, cancellationToken);
                    return;
                }

                if (text.Contains("Ma'lumot", StringComparison.OrdinalIgnoreCase) || text.Contains("Bog'lanish", StringComparison.OrdinalIgnoreCase) || text.Contains("Malumot", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleInfoAsync(botClient, chatId, cancellationToken);
                    return;
                }

                // Noma'lum buyruq kelsa
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Iltimos, pastdagi menyu tugmalaridan birini tanlang:",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Botda xatolik yuz berdi. ChatId: {ChatId}", chatId);
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Kechirasiz, vaqtinchalik xatolik yuz berdi. Qaytadan urinib ko'ring.",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken);
            }
        }

        // /start bosilganda
        private async Task HandleStartAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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

            // Agar oldin ro'yxatdan o'tgan bo'lsa -> To'g'ridan-to'g'ri saytga kirish tugmasini beramiz
            if ((int)authResult.StatusCode is >= 200 and < 300 && authResult.result != null)
            {
                var code = await authService.CreateTelegramLoginCodeAsync(authResult.result.UserId);
                var siteLoginUrl = $"{_websiteUrl}/login?code={code}";

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithUrl("🌐 XalqBozor saytiga kirish (Profil)", siteLoginUrl) }
                });

                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Assalomu alaykum, <b>{authResult.result.Name}</b>! Xush kelibsiz! 😊\n\n" +
                          $"Quyidagi tugmani bosing va <b>XalqBozor</b> do'koniga profilingiz bilan parolsiz kiring:",
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);

                // Pastdagi doimiy menyuni ham ko'rsatamiz
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Do'kon menyusi:",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken);
            }
            else
            {
                // Yangi foydalanuvchi bo'lsa -> Telefon raqamini so'raymiz
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
                    text: $"Assalomu alaykum, <b>{message.Chat.FirstName}</b>!\n\n" +
                          $"<b>XalqBozor</b> internet do'koniga xush kelibsiz! 😊\n\n" +
                          $"Tizimdan to'liq foydalanish va profilingizni faollashtirish uchun pastdagi <b>«📱 Telefon raqamni ulashish»</b> tugmasini bosing:",
                    parseMode: ParseMode.Html,
                    replyMarkup: contactButton,
                    cancellationToken: cancellationToken);
            }
        }

        // Kontakt (Telefon raqam) kelganda
        private async Task HandleContactAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var contact = message.Contact!;
            var chatId = message.Chat.Id;

            if (contact.UserId != message.From?.Id)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Iltimos, faqat o'zingizning raqamingizni yuboring (pastdagi tugma orqali).",
                    cancellationToken: cancellationToken);
                return;
            }
            
            //var contact = message.Contact!;

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
                var code = await authService.CreateTelegramLoginCodeAsync(response.result.UserId);
                var siteLoginUrl = $"{_websiteUrl}/login?code={code}";

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithUrl("🚀 XalqBozor saytiga kirish", siteLoginUrl) }
                });

                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Tabriklaymiz, <b>{response.result.Name}</b>! 🎉\n\n" +
                          $"Siz muvaffaqiyatli ro'yxatdan o'tdingiz!\n\n" +
                          $"Saytga parolsiz, to'g'ridan-to‘g‘ri profilingiz bilan kirish uchun quyidagi tugmani bosing:",
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);

                // Pastdagi doimiy menyuni ham beramiz
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Quyidagi bo'limlardan foydalanishingiz mumkin:",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Xatolik yuz berdi: {response.Message}",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken);
            }
        }

        // 🛍 Mahsulotlar katalogi
        private async Task HandleCatalogAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("🛍 Katalogni ochish", $"{_websiteUrl}/products") }
            });

            await botClient.SendMessage(
                chatId: chatId,
                text: "📦 <b>XalqBozor mahsulotlar katalogi:</b>\n\n" +
                      "Eng so'nggi va sifatli tovarlarimizni saytimizda qulay saralash va filtrlar orqali ko'rishingiz mumkin:",
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // 🛒 Savatcha
        private async Task HandleCartAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("🛒 Savatchani ochish", $"{_websiteUrl}/cart") }
            });

            await botClient.SendMessage(
                chatId: chatId,
                text: "🛒 <b>Sizning savatchangiz:</b>\n\n" +
                      "Savatchangizdagi mahsulotlarni ko'rish va xaridni rasmiylashtirish uchun saytga o'ting:",
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // 📋 Buyurtmalarim
        private async Task HandleOrdersAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("📋 Buyurtmalarim holati", $"{_websiteUrl}/orders") }
            });

            await botClient.SendMessage(
                chatId: chatId,
                text: "📋 <b>Mening buyurtmalarim:</b>\n\n" +
                      "Barcha bergan buyurtmalaringiz, ularning holati (Kutilmoqda, Yetkazildi) va to'lovlarini kuzatish:",
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // ℹ️ Ma'lumot / Bog'lanish
        private async Task HandleInfoAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("🌐 XalqBozor.uz ga o'tish", _websiteUrl) }
            });

            var text =
                "🏢 <b>XalqBozor - Zamonaviy E-Commerce do'koni</b>\n\n" +
                "📍 <b>Manzil:</b> Toshkent shahar\n" +
                "📞 <b>Aloqa:</b> +998 90 123 45 67\n" +
                $"🌐 <b>Veb-sayt:</b> {_websiteUrl}\n\n" +
                "Savollar yoki takliflar bo'lsa, biz bilan bog'laning!";

            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // Pastki doimiy menyu tugmalari
        private ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("🛍 Mahsulotlar katalogi"), new KeyboardButton("🛒 Savatcha") },
                new[] { new KeyboardButton("📋 Buyurtmalarim"), new KeyboardButton("ℹ️ Ma'lumot / Bog'lanish") }
            })
            {
                ResizeKeyboard = true
            };
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Telegram Polling xatosi. Manba: {Source}", source);
            return Task.CompletedTask;
        }
    }
}
