using IdeaCollectionIdea.Common.Constants;
using Microsoft.Extensions.DependencyInjection;
using IdeaCollectionSystem.ApplicationCore.Entitites;
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
		private readonly IEmailService _emailService;

		private readonly IServiceScopeFactory _scopeFactory;

		public IdeaService(IdeaCollectionDbContext context, UserManager<IdeaUser> userManager, IEmailService emailService, IServiceScopeFactory scopeFactory)
		{
			_context = context;
			_userManager = userManager;
			_emailService = emailService;

			_scopeFactory = scopeFactory;
		}

		// Clouse DAte
		public async Task<bool> IsClosureDatePassedAsync()
		{
			var latestSubmission = await _context.Submissions
				.OrderByDescending(s => s.ClousureDate)
				.FirstOrDefaultAsync();

			if (latestSubmission == null) return true;
			return DateTime.UtcNow > latestSubmission.ClousureDate;
		}

		// Final Clousure date
		public async Task<bool> IsFinalClosureDatePassedAsync(Guid ideaId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Submission)
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea?.Submission == null) return true;
			return DateTime.UtcNow > idea.Submission.FinalClousureDate;
		}

		// Create Idea
		public async Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId)
		{
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null) return false;

			
			// 1. VALIDATE TERMS AND CONDITIONS ACCEPTANCE (Simplified)
			
			if (!dto.HasAcceptedTerms)
			{
				return false; // Đá văng ngay nếu user không chịu đồng ý điều khoản
			}

			
			// 2. DETERMINE DEPARTMENT ID
			
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

			
			// 3. VALIDATE SUBMISSION TIMELINE
			
			Submission? submission;
			if (dto.SubmissionId != Guid.Empty)
				submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);
			else
				submission = await _context.Submissions.OrderByDescending(s => s.ClousureDate).FirstOrDefaultAsync();

			if (submission == null || DateTime.UtcNow > submission.ClousureDate) return false;

			
			// 4. CREATE IDEA ENTITY
			
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

			
			// 5. HANDLE ATTACHED DOCUMENTS
			
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

			// Commit Idea và Documents vào DB
			await _context.SaveChangesAsync();

			
			// 6. PROCESS SENDING EMAIL NOTIFICATION TO QA COORDINATOR 
			

			// Handle anonymity for the email body
			var authorName = dto.IsAnonymous ? "An anonymous employee" : ideaUser.Name;
			var ideaText = idea.Text;
			var deptId = departmentId;

			// Fire-and-forget background task using IServiceScopeFactory
			_ = Task.Run(async () =>
			{
				try
				{
					using var scope = _scopeFactory.CreateScope();
					var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdeaUser>>();
					var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

					// Target the QA Coordinator of the specific department
					var coordinators = await userManager.GetUsersInRoleAsync(RoleConstants.QACoordinator);
					var deptCoordinator = coordinators.FirstOrDefault(u => u.DepartmentId == deptId);

					if (deptCoordinator != null && !string.IsNullOrEmpty(deptCoordinator.Email))
					{
						string subject = "💡 [Idea System] A new idea requires your review!";

						string body = $@"
			<div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eaeaea; border-radius: 8px;'>
				<h3 style='color: #2c3e50;'>Hello {deptCoordinator.Name},</h3>
				<p><strong>{authorName}</strong> from your Department has just submitted a new idea to the system.</p>
				<div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #007bff; margin: 15px 0;'>
					<i>""{ideaText}""</i>
				</div>
				<p>Please log in to the system to review the attached files and evaluate it.</p>
				<br/>
				<p style='font-size: 12px; color: #888;'>This is an automated message, please do not reply to this email.</p>
			</div>";

						await emailService.SendEmailAsync(deptCoordinator.Email, subject, body);
						Console.WriteLine($"[EMAIL SUCCESS]: Notification sent to QA {deptCoordinator.Email}");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[EMAIL ERROR]: Background email task failed - {ex.Message}");
				}
			});

			return true;
		}

		//  Get all IDEAS 
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

		// Pagination, sorting, filtering
		public async Task<PagedResult<IdeaInfoDto>> GetIdeasPagedAsync(IdeaQueryParameters parameters, string userId)
		{

			var query = _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
				.Include(i => i.IdeaReactions)
				.AsQueryable();

			//  filtering: 
			if (parameters.SubmissionId.HasValue && parameters.SubmissionId.Value != Guid.Empty)
			{
				query = query.Where(i => i.SubmissionId == parameters.SubmissionId.Value);
			}


			switch (parameters.SortBy?.ToLower())
			{
				case "popular":
					query = query.OrderByDescending(i =>
						i.IdeaReactions.Count(r => r.Reaction == "thumbs_up") -
						i.IdeaReactions.Count(r => r.Reaction == "thumbs_down"));
					break;

				case "viewed":
					query = query.OrderByDescending(i => i.ViewCount);
					break;

				case "latest_comments":
					query = query.OrderByDescending(i => i.Comments.Any() ? i.Comments.Max(c => c.CreatedAt) : DateTime.MinValue);
					break;

				case "latest":
				default:
					query = query.OrderByDescending(i => i.CreatedAt);
					break;
			}

			//  PAGINATIOn
			int totalCount = await query.CountAsync();


			var ideas = await query
				.Skip((parameters.PageNumber - 1) * parameters.PageSize)
				.Take(parameters.PageSize)
				.ToListAsync();

			// 5. MAP SANG DTO
			var resultItems = new List<IdeaInfoDto>();
			foreach (var idea in ideas)
			{
				var author = "Unknown";
				if (!idea.IsAnonymous)
				{
					var user = await _userManager.FindByIdAsync(idea.UserId);
					author = user?.Name ?? user?.Email ?? "Unknown";
				}
				else author = "Anonymous";

				bool canComment = idea.Submission != null && DateTime.UtcNow <= idea.Submission.FinalClousureDate;

				resultItems.Add(new IdeaInfoDto
				{
					Id = idea.Id,
					Text = idea.Text,
					CategoryName = idea.Category?.Name ?? "No Category",
					DepartmentName = idea.Department?.Name ?? "",
					AuthorName = author,
					CreatedDate = idea.CreatedAt,
					IsAnonymous = idea.IsAnonymous,
					ViewCount = idea.ViewCount, // Đã có View Count ở bước trước
					ThumbsUpCount = idea.IdeaReactions.Count(r => r.Reaction == "thumbs_up"),
					ThumbsDownCount = idea.IdeaReactions.Count(r => r.Reaction == "thumbs_down"),
					CommentCount = idea.Comments.Count,
					CanComment = canComment
				});
			}

			return new PagedResult<IdeaInfoDto>
			{
				Items = resultItems,
				TotalCount = totalCount,
				PageNumber = parameters.PageNumber,
				PageSize = parameters.PageSize
			};
		}

		// Get Ideas By Department
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

		// Get Ideas By Staff
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

		//  GET IDEA DETAILS (With Comments) 
		public async Task<IdeaInfoDto?> GetIdeaDetailAsync(Guid ideaId, string userId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments) // Kéo Comment lên
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea == null) return null;

			idea.ViewCount += 1;
			await _context.SaveChangesAsync();

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

		//  INTERACT & OTHERS 
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