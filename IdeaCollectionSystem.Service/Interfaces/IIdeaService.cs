using IdeaCollectionSystem.Service.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IIdeaService
	{
		Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId);
		Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId);
		Task<string?> GetIdeasByUserAsync(string userIdClaim);
		Task<bool> IsClosureDatePassedAsync();
	}
}
