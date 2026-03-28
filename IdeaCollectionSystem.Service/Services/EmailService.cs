using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using IdeaCollectionSystem.Service.Interfaces;

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
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_config["EmailSettings:SenderName"], _config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]!), SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        // --- NEW: Approval Email Template ---
        public async Task SendIdeaApprovedEmailAsync(string toEmail, string authorName, string ideaTitle)
        {
            string subject = $"Your idea '{ideaTitle}' has been approved!";
            string body = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Hello {authorName},</h2>
                    <p>Great news! Your recent idea submission, <strong>'{ideaTitle}'</strong>, has been reviewed and approved by the QA Coordinator.</p>
                    <p>Thank you for your valuable contribution to the system!</p>
                    <br/>
                    <p>Best regards,</p>
                    <p><strong>Idea Collection System Team</strong></p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        // --- NEW: Rejection Email Template ---
        public async Task SendIdeaRejectedEmailAsync(string toEmail, string authorName, string ideaTitle, string rejectionReason)
        {
            string subject = $"Update regarding your idea '{ideaTitle}'";
            string body = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Hello {authorName},</h2>
                    <p>Thank you for submitting your idea, <strong>'{ideaTitle}'</strong>.</p>
                    <p>After careful review, the QA Coordinator has decided not to move forward with this idea at this time.</p>
                    <p><strong>Feedback/Reason:</strong><br/>
                    <span style='color: #d9534f;'>{rejectionReason}</span></p>
                    <p>We highly encourage you to continue submitting your innovative ideas in the future!</p>
                    <br/>
                    <p>Best regards,</p>
                    <p><strong>Idea Collection System Team</strong></p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}