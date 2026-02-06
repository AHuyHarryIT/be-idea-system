using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}