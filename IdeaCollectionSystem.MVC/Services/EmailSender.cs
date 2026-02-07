using Microsoft.AspNetCore.Identity.UI.Services;

namespace IdeaCollectionSystem.MVC.Services
{
	public class EmailSender : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			// TODO: gửi mail thật thì code sau
			return Task.CompletedTask;
		}
	}
}
