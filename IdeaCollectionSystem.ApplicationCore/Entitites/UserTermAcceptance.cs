using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
    public class UserTermAcceptance
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("TermId")]
        public Guid TermId { get; set; }
        public TermVersion? Term { get; set; }
        public DateTime AcceptedAt { get; set; } = DateTime.Now;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<TermVersion> Terms { get; set; } = new List<TermVersion>();
    }
}