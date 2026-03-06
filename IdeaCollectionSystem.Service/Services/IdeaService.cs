using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services
{
	public class IdeaService : IIdeaService
	{
		private readonly IdeaCollectionDbContext _context;
		private readonly UserManager<IdeaUser> _userManager;

		public IdeaService(IdeaCollectionDbContext context, UserManager<IdeaUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		//  Check closure date 
		public async Task<bool> IsClosureDatePassedAsync()
		{
			var latestSubmission = await _context.Submissions
				.OrderByDescending(s => s.ClousureDate)
				.FirstOrDefaultAsync();

			if (latestSubmission == null) return true;
			return DateTime.UtcNow > latestSubmission.ClousureDate;
		}

		//  Create idea — tất cả 4 role đều submit được
		public async Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId)
		{
			// userId đến từ ASP.NET Identity (IdeaUser.Id — string)
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null) return false;

			// Tìm bản ghi User tương ứng trong bảng Users (custom) theo Email
			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.UserName == ideaUser.UserName);

			// Nếu không có bản ghi custom User, vẫn cho phép submit
			// nhưng cần có DepartmentId — dùng Guid.Empty nếu chưa có
			var departmentId = user?.DepartmentId ?? Guid.Empty;

			// Find active submission
			var submission = await _context.Submissions
				.OrderByDescending(s => s.ClousureDate)
				.FirstOrDefaultAsync();

			if (submission == null) return false;

			// Validate CategoryId
			if (dto.CategoryId == Guid.Empty) return false;

			var idea = new Idea
			{
				Id = Guid.NewGuid(),
				Text = dto.Text,
				Description = dto.Description,
				CategoryId = dto.CategoryId,
				DepartmentId = departmentId,
				UserId = user?.Id ?? Guid.Empty,
				SubmissionId = submission.Id,
				IsAnonymous = dto.IsAnonymous,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Ideas.AddAsync(idea);

			// Lưu file documents nếu có
			if (dto.FilePaths != null && dto.FilePaths.Any())
			{
				foreach (var path in dto.FilePaths)
				{
					var doc = new IdeaDocuments
					{
						Id = Guid.NewGuid(),
						IdeaId = idea.Id,
						StoredPath = path,
						OriginalFileName = Path.GetFileName(path),
						MimeType = "",
						FizeSize = 0,
						UploadtedAt = DateTime.UtcNow
					};
					await _context.IdeaDocuments.AddAsync(doc);
				}
			}

			await _context.SaveChangesAsync();
			return true;
		}

		//  Get all ideas (Admin + QAManager)
		public async Task<IEnumerable<IdeaInfoDto>> GetAllIdeasAsync()
		{
			return await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.User)
				.Include(i => i.Department)
				.OrderByDescending(i => i.CreatedAt)
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(),
					Title = i.Text,
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					DepartmentName = i.Department != null ? i.Department.Name : "",
					AuthorName = i.IsAnonymous ? "Anonymous" : (i.User != null ? i.User.FirstName + " " + i.User.LastName : "Unknown"),
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count
				})
				.ToListAsync();
		}

		//  Get ideas by dept (QACoordinator)
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByDepartmentAsync(string userId)
		{
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null) return Enumerable.Empty<IdeaInfoDto>();

			var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == ideaUser.UserName);
			if (user == null) return Enumerable.Empty<IdeaInfoDto>();

			return await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.User)
				.Where(i => i.DepartmentId == user.DepartmentId)
				.OrderByDescending(i => i.CreatedAt)
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(),
					Title = i.Text,
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					DepartmentName = i.Department != null ? i.Department.Name : "",
					AuthorName = i.IsAnonymous ? "Anonymous" : (i.User != null ? i.User.FirstName + " " + i.User.LastName : "Unknown"),
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count
				})
				.ToListAsync();
		}

		//  Get ideas by staff (lấy theo UserId từ Identity)
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId)
		{
			// userId từ Identity là string (Guid dạng string)
			if (!Guid.TryParse(userId, out var userGuid))
				return Enumerable.Empty<IdeaInfoDto>();

			// Tìm user custom để lấy mapping, nhưng vẫn dùng userId từ Identity làm khóa
			// Ideas.UserId được gán từ user.Id (custom User bảng Users)
			// Nếu không có, fallback tìm theo ideas được tạo gần đây
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null) return Enumerable.Empty<IdeaInfoDto>();

			var customUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == ideaUser.UserName);
			if (customUser == null)
			{
				// Chưa có custom user profile, trả về empty
				return Enumerable.Empty<IdeaInfoDto>();
			}

			return await _context.Ideas
				.Include(i => i.Category)
				.Where(i => i.UserId == customUser.Id)
				.OrderByDescending(i => i.CreatedAt)
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(),
					Title = i.Text,
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count
				})
				.ToListAsync();
		}

		//  Get idea detail
		public async Task<IdeaInfoDto?> GetIdeaDetailAsync(int ideaId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.User)
				.Include(i => i.Department)
				.Include(i => i.Comments)
				.FirstOrDefaultAsync(i => i.Id.GetHashCode() == ideaId);

			if (idea == null) return null;

			return new IdeaInfoDto
			{
				Id = idea.Id.GetHashCode(),
				Title = idea.Text,
				CategoryName = idea.Category?.Name ?? "No Category",
				DepartmentName = idea.Department?.Name ?? "",
				AuthorName = idea.IsAnonymous ? "Anonymous" : (idea.User != null ? idea.User.FirstName + " " + idea.User.LastName : "Unknown"),
				CreatedDate = idea.CreatedAt,
				IsAnonymous = idea.IsAnonymous,
				ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == idea.Id && r.Reaction == "thumbs_up"),
				ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == idea.Id && r.Reaction == "thumbs_down"),
				CommentCount = idea.Comments.Count
			};
		}

		//  Vote
		public async Task<bool> VoteIdeaAsync(int ideaId, string userId, bool isThumbsUp)
		{
			if (!Guid.TryParse(userId, out var userGuid))
				return false;

			var idea = await _context.Ideas
				.FirstOrDefaultAsync(i => i.Id.GetHashCode() == ideaId);
			if (idea == null) return false;

			var existingReaction = await _context.IdeaReactions
				.FirstOrDefaultAsync(r => r.IdeaId == idea.Id && r.UserId == userGuid);

			var reactionType = isThumbsUp ? "thumbs_up" : "thumbs_down";

			if (existingReaction != null)
			{
				if (existingReaction.Reaction == reactionType)
				{
					_context.IdeaReactions.Remove(existingReaction);
				}
				else
				{
					existingReaction.Reaction = reactionType;
					existingReaction.UpdatedAt = DateTime.UtcNow;
				}
			}
			else
			{
				var reaction = new IdeaReactions
				{
					Id = Guid.NewGuid(),
					IdeaId = idea.Id,
					UserId = userGuid,
					Reaction = reactionType,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};
				await _context.IdeaReactions.AddAsync(reaction);
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<string?> GetIdeasByUserAsync(string userIdClaim)
		{
			if (!Guid.TryParse(userIdClaim, out var userGuid))
				return null;

			var ideas = await _context.Ideas
				.Where(i => i.UserId == userGuid)
				.Select(i => i.Text)
				.ToListAsync();

			return ideas.Count == 0 ? null : string.Join(", ", ideas);
		}
	}
}