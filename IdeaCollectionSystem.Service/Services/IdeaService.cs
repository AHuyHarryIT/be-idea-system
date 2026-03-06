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

		// Check closure date
		public async Task<bool> IsClosureDatePassedAsync()
		{
			var latestSubmission = await _context.Submissions
				.OrderByDescending(s => s.ClousureDate)
				.FirstOrDefaultAsync();

			if (latestSubmission == null) return true;
			return DateTime.UtcNow > latestSubmission.ClousureDate;
		}

		// Create idea — dùng IdeaUser từ Identity, không cần custom User
		public async Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId)
		{
			// Tìm IdeaUser từ Identity
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null) return false;

			// Xác định DepartmentId: ưu tiên dto, fallback IdeaUser.DepartmentId, rồi Department đầu tiên
			Guid departmentId;
			if (dto.DepartmentId != Guid.Empty)
			{
				departmentId = dto.DepartmentId;
			}
			else if (ideaUser.DepartmentId.HasValue && ideaUser.DepartmentId.Value != Guid.Empty)
			{
				departmentId = ideaUser.DepartmentId.Value;
			}
			else
			{
				var firstDept = await _context.Departments.FirstOrDefaultAsync();
				if (firstDept == null) return false;
				departmentId = firstDept.Id;
			}

			var departmentExists = await _context.Departments.AnyAsync(d => d.Id == departmentId);
			if (!departmentExists) return false;

			if (dto.CategoryId == Guid.Empty) return false;

			// Lấy Submission: ưu tiên dto.SubmissionId, fallback lấy mới nhất
			Submission? submission;
			if (dto.SubmissionId != Guid.Empty)
				submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);
			else
				submission = await _context.Submissions.OrderByDescending(s => s.ClousureDate).FirstOrDefaultAsync();

			if (submission == null) return false;

			// Kiểm tra Submission còn trong thời hạn
			if (DateTime.UtcNow > submission.ClousureDate) return false;

			var idea = new Idea
			{
				Id = Guid.NewGuid(),
				Text = dto.Text,
				Description = dto.Description,
				CategoryId = dto.CategoryId,
				DepartmentId = departmentId,
				UserId = userId,               // string — Identity user ID
				SubmissionId = submission.Id,
				IsAnonymous = dto.IsAnonymous,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Ideas.AddAsync(idea);

			if (dto.FilePaths != null && dto.FilePaths.Any())
			{
				foreach (var path in dto.FilePaths)
				{
					var doc = new IdeaDocument
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

		// Get all ideas (Admin + QAManager)
		public async Task<IEnumerable<IdeaInfoDto>> GetAllIdeasAsync()
		{
			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			var result = new List<IdeaInfoDto>();
			foreach (var i in ideas)
			{
				var author = "Unknown";
				if (!i.IsAnonymous)
				{
					var user = await _userManager.FindByIdAsync(i.UserId);
					author = user?.Name ?? user?.Email ?? "Unknown";
				}
				else
				{
					author = "Anonymous";
				}

				result.Add(new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(),
					Text = i.Text,
					CategoryName = i.Category?.Name ?? "No Category",
					DepartmentName = i.Department?.Name ?? "",
					AuthorName = author,
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count
				});
			}
			return result;
		}

		// Get ideas by dept (QACoordinator)
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByDepartmentAsync(string userId)
		{
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null || ideaUser.DepartmentId == null) return Enumerable.Empty<IdeaInfoDto>();

			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Where(i => i.DepartmentId == ideaUser.DepartmentId.Value)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			var result = new List<IdeaInfoDto>();
			foreach (var i in ideas)
			{
				var user = await _userManager.FindByIdAsync(i.UserId);
				result.Add(new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(),
					Text = i.Text,
					CategoryName = i.Category?.Name ?? "No Category",
					DepartmentName = i.Department?.Name ?? "",
					AuthorName = i.IsAnonymous ? "Anonymous" : (user?.Name ?? user?.Email ?? "Unknown"),
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count
				});
			}
			return result;
		}

		// Get ideas by staff
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId)
		{
			return await _context.Ideas
				.Include(i => i.Category)
				.Where(i => i.UserId == userId)
				.OrderByDescending(i => i.CreatedAt)
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(),
					Text = i.Text,
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count
				})
				.ToListAsync();
		}

		// Get idea detail
		public async Task<IdeaInfoDto?> GetIdeaDetailAsync(int ideaId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Comments)
				.FirstOrDefaultAsync(i => i.Id.GetHashCode() == ideaId);

			if (idea == null) return null;

			var user = await _userManager.FindByIdAsync(idea.UserId);

			return new IdeaInfoDto
			{
				Id = idea.Id.GetHashCode(),
				Text = idea.Text,
				CategoryName = idea.Category?.Name ?? "No Category",
				DepartmentName = idea.Department?.Name ?? "",
				AuthorName = idea.IsAnonymous ? "Anonymous" : (user?.Name ?? user?.Email ?? "Unknown"),
				CreatedDate = idea.CreatedAt,
				IsAnonymous = idea.IsAnonymous,
				ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == idea.Id && r.Reaction == "thumbs_up"),
				ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == idea.Id && r.Reaction == "thumbs_down"),
				CommentCount = idea.Comments.Count
			};
		}

		// Vote
		public async Task<bool> VoteIdeaAsync(int ideaId, string userId, bool isThumbsUp)
		{
			var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id.GetHashCode() == ideaId);
			if (idea == null) return false;

			// UserId là string — so sánh trực tiếp, không cần Guid.TryParse
			var existingReaction = await _context.IdeaReactions
				.FirstOrDefaultAsync(r => r.IdeaId == idea.Id && r.UserId == userId);

			var reactionType = isThumbsUp ? "thumbs_up" : "thumbs_down";

			if (existingReaction != null)
			{
				if (existingReaction.Reaction == reactionType)
					_context.IdeaReactions.Remove(existingReaction);
				else
				{
					existingReaction.Reaction = reactionType;
					existingReaction.UpdatedAt = DateTime.UtcNow;
				}
			}
			else
			{
				await _context.IdeaReactions.AddAsync(new IdeaReaction
				{
					Id = Guid.NewGuid(),
					IdeaId = idea.Id,
					UserId = userId,
					Reaction = reactionType,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				});
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<string?> GetIdeasByUserAsync(string userId)
		{
			var ideas = await _context.Ideas
				.Where(i => i.UserId == userId)
				.Select(i => i.Text)
				.ToListAsync();

			return ideas.Count == 0 ? null : string.Join(", ", ideas);
		}
	}
}