using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
    public class IdeaView
    {

        [Key]
        public Guid Id { get; set; }
        public DateTime VistiTime { get; set; } = DateTime.Now;

    }
}