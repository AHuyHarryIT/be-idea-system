using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdeaCollectionSystem.Service.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            IEmailQueue emailQueue,
            IServiceProvider serviceProvider,
            ILogger<EmailBackgroundService> logger)
        {
            _emailQueue = emailQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hệ thống gửi Email chạy ngầm đã khởi động.");

            // Vòng lặp vô hạn chạy ngầm chừng nào app còn bật
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Đợi lấy email từ Queue (nếu Queue rỗng, nó sẽ tạm ngủ không tốn CPU)
                    var emailRequest = await _emailQueue.DequeueEmailAsync(stoppingToken);

                    // Khởi tạo scope để gọi IEmailSender (vì EmailSender đã đăng ký dạng HttpClient/Scoped)
                    using var scope = _serviceProvider.CreateScope();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    _logger.LogInformation("Đang gửi email ngầm tới {Email}", emailRequest.ToEmail);

                    // Thực thi việc gửi mail qua Brevo API
                    await emailSender.SendEmailAsync(emailRequest.ToEmail, emailRequest.Subject, emailRequest.Message);
                }
                catch (OperationCanceledException)
                {
                    // App đang tắt, thoát vòng lặp an toàn
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong quá trình gửi email ngầm.");
                }
            }
        }
    }
}   