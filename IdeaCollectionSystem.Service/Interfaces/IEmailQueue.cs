using IdeaCollectionSystem.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Interfaces
{
    public interface IEmailQueue
    {
        ValueTask QueueEmailAsync(EmailMessage email);
        ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken);
    }
}
