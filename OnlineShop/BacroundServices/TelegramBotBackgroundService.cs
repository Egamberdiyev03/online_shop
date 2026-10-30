using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace OnlineShop.BacroundServices
{
    public class TelegramBotBackgroundService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUpdateHandler _updateHandler;
        private readonly ILogger<TelegramBotBackgroundService> _logger;
        public TelegramBotBackgroundService(
            IConfiguration config,
            IUpdateHandler updateHandler,
            ILogger<TelegramBotBackgroundService> logger)
        {
            _updateHandler = updateHandler;
            _logger = logger;
            var token = config["TelegramBot:BotToken"];
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Telegram BotToken appsettings.json da ko'rsatilmagan!");
            }
            _botClient = new TelegramBotClient(token ?? string.Empty);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message },
                DropPendingUpdates = true // Bot o'chib qolgandagi eski xabarlarni tashlab yuboradi
            };
            _logger.LogInformation("Telegram Bot muvaffaqiyatli ishga tushdi...");
            await _botClient.ReceiveAsync(
                updateHandler: _updateHandler,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );
        }
    }
}
