using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestController : ControllerBase
	{
		private readonly IEmailService _emailService;

		// Tiêm (Inject) IEmailService vào Controller
		public TestController(IEmailService emailService)
		{
			_emailService = emailService;
		}

		[HttpPost("send-email")]
		public async Task<IActionResult> TestSendEmail([FromQuery] string receiveEmail)
		{
			try
			{
				string subject = "🎉 Chúc mừng! Backend Idea System đã hoạt động!";
				string htmlBody = @"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                        <h2 style='color: #2e6c80;'>Hệ thống Email đã được kết nối!</h2>
                        <p>Chào bạn,</p>
                        <p>Nếu bạn nhận được email này, nghĩa là <b>EmailService</b> kết hợp với Gmail SMTP trong dự án ASP.NET Core của bạn đã chạy thành công 100%.</p>
                        <hr/>
                        <p><i>Hệ thống được gửi tự động, vui lòng không trả lời.</i></p>
                    </div>";

				// Gọi hàm gửi mail
				await _emailService.SendEmailAsync(receiveEmail, subject, htmlBody);

				return Ok(new { Message = $"Đã gửi email test thành công tới {receiveEmail}!" });
			}
			catch (Exception ex)
			{
				// Nếu cấu hình sai, nó sẽ in ra lỗi chi tiết ở đây
				return BadRequest(new { Error = "Lỗi gửi mail", Detail = ex.Message });
			}
		}
	}
}