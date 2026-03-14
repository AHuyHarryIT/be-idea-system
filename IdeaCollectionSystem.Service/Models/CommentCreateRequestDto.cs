using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Models
{
	public class CommentCreateRequestDto
	{
		public string Text { get; set; } = string.Empty;
		public bool IsAnonymous { get; set; }
	}
}
