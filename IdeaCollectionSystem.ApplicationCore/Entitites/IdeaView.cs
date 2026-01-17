using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class IdeaView
	{
		public Guid Id { get; set; } 
		public DateTime VistiTime	{ get; set; } = DateTime.Now;

	}
}
