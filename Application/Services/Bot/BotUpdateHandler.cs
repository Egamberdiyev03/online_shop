using Application.DTOs.Auth;
using Application.Interfaces;
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

        // Saytingiz manzili (Ishlab chiqishda localhost:4200, ertaga serverda o'z domeni)
        private const string WebsiteUrl = "http://localhost:4200";

        public BotUpdateHandler(IServiceProvider serviceProvider, ILogger<BotUpdateHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
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

                // 3. Pastdagi menyu tugmalari:
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

                // Noma'lum buyruq kelganda
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
                    text: $"Kechirasiz, xatolik yuz berdi: {ex.Message}",
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
                Name = message.Chat.FirstName ?? "Mijoz",
                TelegramUsername = message.Chat.Username
            };

            var authResult = await authService.TelegramAuthAsync(checkDto);

            // Agar oldin ro'yxatdan o'tgan bo'lsa (Login holati):
            if ((int)authResult.StatusCode is >= 200 and < 300 && !string.IsNullOrEmpty(authResult.result?.Token))
            {
                var token = authResult.result.Token;
                var siteLoginUrl = $"{WebsiteUrl}/login?token={token}";
                var inlineKeyboard = CreateSafeInlineKeyboard("🌐 Saytga kirish (Profil)", siteLoginUrl);

                var welcomeText =
                    $"Assalomu alaykum, <b>{authResult.result.Name}</b>! Xush kelibsiz! 😊\n\n" +
                    $"🌐 <b>Saytga profilingiz bilan kirish uchun havola:</b>\n" +
                    $"👉 <a href=\"{siteLoginUrl}\"><b>OnlineShop saytiga kirish (Avtologin)</b></a>\n\n" +
                    $"<i>(Yuqoridagi ko'k havola ustiga bosing, profilingizga parolsiz kirasiz)</i>";

                await botClient.SendMessage(
                    chatId: chatId,
                    text: welcomeText,
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);

                // Pastdagi menyuni chiqarib qo'yamiz
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Boshqaruv menyusi:",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken);
            }
            else
            {
                // Yangi bo'lsa -> Raqamni so'raymiz
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
                    text: $"Assalomu alaykum, <b>{message.Chat.FirstName}</b>!\n\nOnlineShop do'koniga xush kelibsiz. Tizimdan to'liq foydalanish uchun pastdagi tugma orqali telefon raqamingizni yuboring:",
                    parseMode: ParseMode.Html,
                    replyMarkup: contactButton,
                    cancellationToken: cancellationToken);
            }
        }

        // Telefon raqam kelganda (Ro'yxatdan o'tish)
        private async Task HandleContactAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var contact = message.Contact!;

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
                var token = response.result.Token;
                var siteLoginUrl = $"{WebsiteUrl}/login?token={token}";
                var inlineKeyboard = CreateSafeInlineKeyboard("🚀 Saytga profilingiz bilan kirish", siteLoginUrl);

                var successText =
                    $"Tabriklaymiz, <b>{response.result.Name}</b>! 🎉\n\n" +
                    $"Siz muvaffaqiyatli ro'yxatdan o'tdingiz!\n\n" +
                    $"🌐 <b>Saytga profilingiz bilan kirish uchun havola:</b>\n" +
                    $"👉 <a href=\"{siteLoginUrl}\"><b>OnlineShop saytiga kirish (Avtologin)</b></a>\n\n" +
                    $"<i>(Havola ustiga bosing, profilingiz avtomatik ochiladi)</i>";

                await botClient.SendMessage(
                    chatId: chatId,
                    text: successText,
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);

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
            var url = $"{WebsiteUrl}/products";
            var inlineKeyboard = CreateSafeInlineKeyboard("🛍 Katalogni saytda ochish", url);

            var text =
                "📦 <b>Bizning mahsulotlar katalogimiz:</b>\n\n" +
                "Eng so'nggi va sifatli tovarlarimizni saytimizda saralash va filtrlar bilan ko'rishingiz mumkin:\n\n" +
                $"👉 <a href=\"{url}\"><b>Mahsulotlar katalogini ochish</b></a>";

            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // 🛒 Savatcha
        private async Task HandleCartAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var url = $"{WebsiteUrl}/cart";
            var inlineKeyboard = CreateSafeInlineKeyboard("🛒 Savatchani ochish", url);

            var text =
                "🛒 <b>Sizning savatchangiz:</b>\n\n" +
                "Savatchangizdagi mahsulotlarni ko'rish va buyurtma berish uchun:\n\n" +
                $"👉 <a href=\"{url}\"><b>Savatchani saytda ochish</b></a>";

            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // 📋 Buyurtmalarim
        private async Task HandleOrdersAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var url = $"{WebsiteUrl}/orders";
            var inlineKeyboard = CreateSafeInlineKeyboard("📋 Buyurtmalar holatini ko'rish", url);

            var text =
                "📋 <b>Mening buyurtmalarim:</b>\n\n" +
                "Barcha bergan buyurtmalaringiz, ularning holati (Kutilmoqda, Yetkazildi) va cheklarni kuzatish uchun:\n\n" +
                $"👉 <a href=\"{url}\"><b>Buyurtmalarim sahifasini ochish</b></a>";

            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // ℹ️ Ma'lumot / Bog'lanish
        private async Task HandleInfoAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var text =
                "🏢 <b>OnlineShop - Zamonaviy E-Commerce do'koni</b>\n\n" +
                "📍 <b>Manzil:</b> Toshkent shahar\n" +
                "📞 <b>Aloqa:</b> +998 90 123 45 67\n" +
                $"🌐 <b>Veb-sayt:</b> <a href=\"{WebsiteUrl}\">OnlineShop Web</a>\n\n" +
                "Savollar yoki takliflar bo'lsa, biz bilan bog'laning!";

            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: cancellationToken);
        }

        // Xavfsiz Inline tugma yasash (Localhost bo'lsa xato bermaslik uchun null qaytaradi, real domenda tugma chiqaradi)
        private static InlineKeyboardMarkup? CreateSafeInlineKeyboard(string buttonText, string url)
        {
            if (url.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl(buttonText, url) }
            });
        }

        // Doimiy pastki menyu tugmalari
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