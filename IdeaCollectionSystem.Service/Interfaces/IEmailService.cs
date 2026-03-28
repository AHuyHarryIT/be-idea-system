namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IEmailService
	{
		Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendIdeaApprovedEmailAsync(string toEmail, string authorName, string ideaTitle);
        Task SendIdeaRejectedEmailAsync(string toEmail, string authorName, string ideaTitle, string rejectionReason);
    }
}