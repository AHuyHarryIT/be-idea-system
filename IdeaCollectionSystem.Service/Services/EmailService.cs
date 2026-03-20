using MailKit.Net.Smtp;
using MailKit.Security; // Bắt buộc phải có để dùng StartTls
using Microsoft.Extensions.Configuration;
using MimeKit;
using IdeaCollectionSystem.Service.Interfaces;

namespace IdeaCollectionSystem.Service.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _config;

		// DI Container sẽ tự động tiêm IConfiguration vào đây để lấy thông tin từ appsettings.json
		public EmailService(IConfiguration config)
		{
			_config = config;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			// 1. Khởi tạo bức thư
			var email = new MimeMessage();
			email.From.Add(new MailboxAddress(_config["EmailSettings:SenderName"], _config["EmailSettings:SenderEmail"]));
			email.To.Add(MailboxAddress.Parse(toEmail));
			email.Subject = subject;

		
			var builder = new BodyBuilder { HtmlBody = body };
			email.Body = builder.ToMessageBody();

			using var smtp = new SmtpClient();
			try
			{
				// Port 587 của Gmail BẮT BUỘC phải dùng SecureSocketOptions.StartTls
				await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]!), SecureSocketOptions.StartTls);

				// Xác thực bằng App Password 16 ký tự
				await smtp.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);

				await smtp.SendAsync(email);
			}
			finally
			{
				// Luôn nhớ ngắt kết nối dọn dẹp RAM
				await smtp.DisconnectAsync(true);
			}
		}
	}
}