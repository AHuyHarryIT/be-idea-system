using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Models
{

	public class DepartmentDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
	}


	public class DepartmentCreateDto
	{
	
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
	}


	public class DepartmentUpdateDto
	{
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
	}
}