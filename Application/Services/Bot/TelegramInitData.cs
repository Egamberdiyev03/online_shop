using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Application.Services.Bot
{
    public class TelegramInitData
    {
        public static bool TryValidate(string initData, string botToken, out long telegramId)
        {
            telegramId = 0;
            if (string.IsNullOrWhiteSpace(initData) || initData.Length > 4096)
                return false;

            var parsed = HttpUtility.ParseQueryString(initData);
            var hash = parsed["hash"];
            if (string.IsNullOrEmpty(hash)) return false;

            var dataCheckString = string.Join("\n",
                parsed.AllKeys
                    .Where(k => k != null && k != "hash")
                    .OrderBy(k => k, StringComparer.Ordinal)
                    .Select(k => $"{k}={parsed[k]}"));

            var secretKey = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes("WebAppData"),
                Encoding.UTF8.GetBytes(botToken));

            var computed = Convert.ToHexString(
                HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString)))
                .ToLowerInvariant();

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(computed),
                    Encoding.UTF8.GetBytes(hash.ToLowerInvariant())))
                return false;

            // Eskirgan ma'lumotni rad etish (1 soat)
            if (!long.TryParse(parsed["auth_date"], out var authDate) ||
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() - authDate > 3600)
                return false;

            var userJson = parsed["user"];
            if (string.IsNullOrEmpty(userJson)) return false;

            using var doc = JsonDocument.Parse(userJson);
            telegramId = doc.RootElement.GetProperty("id").GetInt64();
            return true;
        }
    }
}
