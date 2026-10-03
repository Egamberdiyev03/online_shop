using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpCodeAsync(string toEmail, string code)
        {
            var smtp = _configuration.GetSection("Smtp");

            using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
            {
                Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(smtp["Username"], "OnlineShop"),
                Subject = "OnlineShop — Email tasdiqlash kodi",
                Body = $"Sizning tasdiqlash kodingiz: {code}\n\nKod 10 daqiqa davomida amal qiladi.",
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
