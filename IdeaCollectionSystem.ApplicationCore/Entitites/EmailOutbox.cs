using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
    public class EmailOutBox
    {
        [Key]
        public Guid Id { get; set; }
        public string EmailTo { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public enum Status
        {
            PENDING = 0,
            SENT = 1,
            FAILED = 2
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public string Error { get; set; } = string.Empty;

        public Guid IdeaId { get; set; }
        [ForeignKey("IdeaId")]
        public Idea? Idea { get; set; }

        public Guid CommentId { get; set; }
        [ForeignKey("CommentId")]
        public Comment? Comment { get; set; }

        public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}