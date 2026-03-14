using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace IdeaCollectionSystem.Service.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _config;

		public EmailService(IConfiguration config)
		{
			_config = config;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			try
			{
				var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
				var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
				var senderEmail = _config["EmailSettings:SenderEmail"];
				var senderPassword = _config["EmailSettings:SenderPassword"];

				using var client = new SmtpClient(smtpServer, port)
				{
					Credentials = new NetworkCredential(senderEmail, senderPassword),
					EnableSsl = true
				};

				var mailMessage = new MailMessage
				{
					From = new MailAddress(senderEmail!, "Idea Collection System"),
					Subject = subject,
					Body = body,
					IsBodyHtml = true
				};
				mailMessage.To.Add(toEmail);

				await client.SendMailAsync(mailMessage);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fail to send mail: {ex.Message}");
			}
		}
	}
}