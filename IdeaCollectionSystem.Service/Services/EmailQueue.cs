using IdeaCollectionSystem.Service.Interfaces;
using System.Threading.Channels;

namespace IdeaCollectionSystem.Service.Services
{
    // Cấu trúc dữ liệu của 1 email để nhét vào Queue
    public class EmailMessage
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _queue;

        public EmailQueue()
        {
            // Cài đặt hàng đợi chứa tối đa 100 email cùng lúc, nếu đầy thì đợi (tránh tràn RAM)
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<EmailMessage>(options);
        }

        public async ValueTask QueueEmailAsync(EmailMessage email)
        {
            ArgumentNullException.ThrowIfNull(email);
            await _queue.Writer.WriteAsync(email);
        }

        public async ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken)
        {
            // Lấy email ra khỏi hàng đợi
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}