using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace IdeaCollectionSystem.Service.Services
{
	public class IdeaService : IIdeaService
	{
		private readonly IdeaCollectionDbContext _context;
		private readonly UserManager<IdeaUser> _userManager;
		private readonly IEmailService _emailService;

		public IdeaService(IdeaCollectionDbContext context, UserManager<IdeaUser> userManager, IEmailService emailService)
		{
			_context = context;
			_userManager = userManager;
			_emailService = emailService;
		}

		public async Task<bool> IsClosureDatePassedAsync()
		{
			var latestSubmission = await _context.Submissions
				.OrderByDescending(s => s.ClousureDate)
				.FirstOrDefaultAsync();

			if (latestSubmission == null) return true;
			return DateTime.UtcNow > latestSubmission.ClousureDate;
		}

		public async Task<bool> IsFinalClosureDatePassedAsync(Guid ideaId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Submission)
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea?.Submission == null) return true;
			return DateTime.UtcNow > idea.Submission.FinalClousureDate;
		}

		// --- CREATE IDEA & SEND EMAIL ---
		public async Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId)
		{
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null) return false;

			Guid departmentId;
			if (dto.DepartmentId != Guid.Empty) departmentId = dto.DepartmentId;
			else if (ideaUser.DepartmentId.HasValue && ideaUser.DepartmentId.Value != Guid.Empty) departmentId = ideaUser.DepartmentId.Value;
			else
			{
				var firstDept = await _context.Departments.FirstOrDefaultAsync();
				if (firstDept == null) return false;
				departmentId = firstDept.Id;
			}

			if (!await _context.Departments.AnyAsync(d => d.Id == departmentId)) return false;
			if (dto.CategoryId == Guid.Empty) return false;

			Submission? submission;
			if (dto.SubmissionId != Guid.Empty)
				submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);
			else
				submission = await _context.Submissions.OrderByDescending(s => s.ClousureDate).FirstOrDefaultAsync();

			if (submission == null || DateTime.UtcNow > submission.ClousureDate) return false;

			var idea = new Idea
			{
				Id = Guid.NewGuid(),
				Text = dto.Text,
				Description = dto.Description,
				CategoryId = dto.CategoryId,
				DepartmentId = departmentId,
				UserId = userId,
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
						MimeType = "application/octet-stream",
						FizeSize = 0,
						UploadtedAt = DateTime.UtcNow
					};
					await _context.IdeaDocuments.AddAsync(doc);
				}
			}

			await _context.SaveChangesAsync();

			// XỬ LÝ GỬI EMAIL THÔNG BÁO CHO QA COORDINATOR
			try
			{
				var coordinators = await _userManager.GetUsersInRoleAsync(RoleConstants.QACoordinator);
				var deptCoordinator = coordinators.FirstOrDefault(u => u.DepartmentId == departmentId);

				if (deptCoordinator != null && !string.IsNullOrEmpty(deptCoordinator.Email))
				{
					string subject = "Hệ thống: Ý tưởng mới vừa được nộp";
					string body = $"<h3>Xin chào {deptCoordinator.Name},</h3>" +
								  $"<p>Một nhân viên trong Khoa của bạn vừa nộp một ý tưởng mới:</p>" +
								  $"<p><strong>\"{idea.Text}\"</strong></p>" +
								  $"<p>Vui lòng đăng nhập hệ thống để xem xét và đánh giá.</p>";

					_ = _emailService.SendEmailAsync(deptCoordinator.Email, subject, body);
				}
			}
			catch { }

			return true;
		}

		// --- READ IDEAS ---
		public async Task<IEnumerable<IdeaInfoDto>> GetAllIdeasAsync()
		{
			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
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
				else author = "Anonymous";

				bool canComment = i.Submission != null && DateTime.UtcNow <= i.Submission.FinalClousureDate;

				result.Add(new IdeaInfoDto
				{
					Id = i.Id,
					Text = i.Text,
					CategoryName = i.Category?.Name ?? "No Category",
					DepartmentName = i.Department?.Name ?? "",
					AuthorName = author,
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count,
					CanComment = canComment
				});
			}
			return result;
		}

		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByDepartmentAsync(string userId)
		{
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null || ideaUser.DepartmentId == null) return Enumerable.Empty<IdeaInfoDto>();

			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
				.Where(i => i.DepartmentId == ideaUser.DepartmentId.Value)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			var result = new List<IdeaInfoDto>();
			foreach (var i in ideas)
			{
				var user = await _userManager.FindByIdAsync(i.UserId);
				bool canComment = i.Submission != null && DateTime.UtcNow <= i.Submission.FinalClousureDate;

				result.Add(new IdeaInfoDto
				{
					Id = i.Id,
					Text = i.Text,
					CategoryName = i.Category?.Name ?? "No Category",
					DepartmentName = i.Department?.Name ?? "",
					AuthorName = i.IsAnonymous ? "Anonymous" : (user?.Name ?? user?.Email ?? "Unknown"),
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = i.Comments.Count,
					CanComment = canComment
				});
			}
			return result;
		}

		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId)
		{
			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
				.Where(i => i.UserId == userId)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			return ideas.Select(i => new IdeaInfoDto
			{
				Id = i.Id,
				Text = i.Text,
				CategoryName = i.Category?.Name ?? "No Category",
				CreatedDate = i.CreatedAt,
				IsAnonymous = i.IsAnonymous,
				ThumbsUpCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
				ThumbsDownCount = _context.IdeaReactions.Count(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
				CommentCount = i.Comments.Count,
				CanComment = i.Submission != null && DateTime.UtcNow <= i.Submission.FinalClousureDate,
			}).ToList();
		}

		// --- GET IDEA DETAILS (With Comments) ---
		public async Task<IdeaInfoDto?> GetIdeaDetailAsync(Guid ideaId, string userId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments) // Kéo Comment lên
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea == null) return null;

			var user = await _userManager.FindByIdAsync(idea.UserId);
			bool canComment = idea.Submission != null && DateTime.UtcNow <= idea.Submission.FinalClousureDate;

			var commentDtos = new List<CommentDto>();
			foreach (var c in idea.Comments.OrderByDescending(c => c.CreatedAt))
			{
				string commentAuthorName = "Anonymous";
				if (!c.IsAnonymous)
				{
					var commentUser = await _userManager.FindByIdAsync(c.UserId);
					commentAuthorName = commentUser?.Name ?? commentUser?.Email ?? "Unknown";
				}

				commentDtos.Add(new CommentDto
				{
					Id = c.Id,
					Text = c.Text ?? "",
					CreatedDate = c.CreatedAt,
					IsAnonymous = c.IsAnonymous,
					AuthorName = commentAuthorName
				});
			}

			return new IdeaInfoDto
			{
				Id = idea.Id,
				Text = idea.Text,
				CategoryName = idea.Category?.Name ?? "No Category",
				DepartmentName = idea.Department?.Name ?? "",
				AuthorName = idea.IsAnonymous ? "Anonymous" : (user?.Name ?? user?.Email ?? "Unknown"),
				CreatedDate = idea.CreatedAt,
				IsAnonymous = idea.IsAnonymous,
				ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == idea.Id && r.Reaction == "thumbs_up"),
				ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == idea.Id && r.Reaction == "thumbs_down"),
				CommentCount = idea.Comments.Count,
				CanComment = canComment,
				Comments = commentDtos
			};
		}


		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync()
		{
			// Lọc ra các Idea không có bất kỳ Comment nào
			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
				.Where(i => i.Comments.Count == 0) // Điều kiện lọc
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
				else author = "Anonymous";

				result.Add(new IdeaInfoDto
				{
					Id = i.Id,
					Text = i.Text,
					CategoryName = i.Category?.Name ?? "No Category",
					DepartmentName = i.Department?.Name ?? "",
					AuthorName = author,
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous,
					ThumbsUpCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up"),
					ThumbsDownCount = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down"),
					CommentCount = 0, // Chắc chắn là 0
					CanComment = i.Submission != null && DateTime.UtcNow <= i.Submission.FinalClousureDate
				});
			}
			return result;
		}

		// --- INTERACT & OTHERS ---
		public async Task<bool> VoteIdeaAsync(Guid ideaId, string userId, bool isThumbsUp)
		{
			var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id == ideaId);
			if (idea == null) return false;

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

		public async Task<bool> AddCommentAsync(Guid ideaId, string userId, string text, bool isAnonymous)
		{
			if (await IsFinalClosureDatePassedAsync(ideaId)) return false;

			var comment = new Comment
			{
				Id = Guid.NewGuid(),
				IdeaId = ideaId,
				UserId = userId,
				Text = text,
				IsAnonymous = isAnonymous,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Comments.AddAsync(comment);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<string?> GetIdeasByUserAsync(string userId)
		{
			var ideas = await _context.Ideas
				.Where(i => i.UserId == userId)
				.Include(i => i.Comments)
				.Select(i => i.Text)
				.ToListAsync();

			return ideas.Count == 0 ? null : string.Join(", ", ideas);
		}

		// Các hàm Overload cũ (Fix lỗi crash Guid nếu lỡ bị gọi)
		public async Task<bool> VoteIdeaAsync(int ideaId, string userId, bool isThumbsUp)
		{
			return await VoteIdeaAsync(new Guid(ideaId.ToString("D32")), userId, isThumbsUp);
		}

		public async Task<IdeaInfoDto?> GetIdeaDetailAsync(int ideaId)
		{
			return await GetIdeaDetailAsync(new Guid(ideaId.ToString("D32")), string.Empty);
		}
	}
}