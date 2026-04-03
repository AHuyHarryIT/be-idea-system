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

		#region PRIVATE HELPER METHOD
	
		private async Task<IdeaInfoDto> MapToDtoAsync(Idea idea)
		{
			var author = "Anonymous";
			if (!idea.IsAnonymous)
			{
				var user = await _userManager.FindByIdAsync(idea.UserId);
				author = user?.Name ?? user?.Email ?? "Unknown";
			}

			bool canComment = idea.Submission != null && DateTime.UtcNow <= idea.Submission.FinalClosureDate;

			var dto = new IdeaInfoDto
			{
				Id = idea.Id,
				Title = idea.Title,
				Description = idea.Description, 
				CategoryName = idea.Category?.Name ?? "No Category",
				DepartmentName = idea.Department?.Name ?? "",
				AuthorName = author,
				CreatedDate = idea.CreatedAt,
				IsAnonymous = idea.IsAnonymous,
				ViewCount = idea.ViewCount,
				ReviewStatus = idea.ReviewStatus,
				ThumbsUpCount = idea.IdeaReactions?.Count(r => r.Reaction == "thumbs_up") ?? 0,
				ThumbsDownCount = idea.IdeaReactions?.Count(r => r.Reaction == "thumbs_down") ?? 0,
				CommentCount = idea.Comments?.Count ?? 0,
				CanComment = canComment,

			
				Comments = idea.Comments?.Select(c => new CommentDto
				{
					Id = c.Id,
					Content = c.Content,
					CreatedDate = c.CreatedAt,
					IsAnonymous = c.IsAnonymous,
					AuthorName = c.IsAnonymous ? "Anonymous" : (c.User?.Name ?? "Unknown User")
				}).OrderByDescending(c => c.CreatedDate).ToList() ?? new List<CommentDto>()
			};

			return dto;
		}
		
		#endregion



		// Check  Closure date
		public async Task<bool> IsClosureDatePassedAsync()
		{
			var latestSubmission = await _context.Submissions
				.OrderByDescending(s => s.ClosureDate)
				.FirstOrDefaultAsync();

			if (latestSubmission == null) return true;

			return DateTime.UtcNow.Date > latestSubmission.ClosureDate.Date;
		}

		// Check Final Closure date
		public async Task<bool> IsFinalClosureDatePassedAsync(Guid ideaId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Submission)
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea?.Submission == null) return true;

			return DateTime.UtcNow.Date > idea.Submission.FinalClosureDate.Date;
		}

		// CREATE IDEA
		public async Task<Guid?> CreateIdeaAsync(IdeaCreateDto dto, string userId)
		{
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null) return null;

			if (!dto.HasAcceptedTerms)
				throw new ArgumentException("You must agree to the Terms and Conditions before submitting an idea!");

			Guid departmentId;
			if (dto.DepartmentId != Guid.Empty) departmentId = dto.DepartmentId;
			else if (ideaUser.DepartmentId.HasValue && ideaUser.DepartmentId.Value != Guid.Empty) departmentId = ideaUser.DepartmentId.Value;
			else
			{
				var firstDept = await _context.Departments.FirstOrDefaultAsync();
				if (firstDept == null) return null;
				departmentId = firstDept.Id;
			}

			if (!await _context.Departments.AnyAsync(d => d.Id == departmentId)) return null;
			if (dto.CategoryId == Guid.Empty) return null;

			Submission? submission;
			if (dto.SubmissionId != Guid.Empty)
				submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);
			else
				submission = await _context.Submissions.OrderByDescending(s => s.ClosureDate).FirstOrDefaultAsync();

			if (submission == null || DateTime.UtcNow.Date > submission.ClosureDate.Date) return null;

			var idea = new Idea
			{
				Id = Guid.NewGuid(),
				Title = dto.Title,
				Description = dto.Description,
				CategoryId = dto.CategoryId,
				DepartmentId = departmentId,
				UserId = userId,
				SubmissionId = submission.Id,
				IsAnonymous = dto.IsAnonymous,
				ReviewStatus = ReviewStatus.PENDING, 
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Ideas.AddAsync(idea);

			// Handle Files
			if (dto.UploadedFiles != null && dto.UploadedFiles.Any())
			{
				var allowedExtensions = new[] { ".pdf" };
				var maxFileSize = 5 * 1024 * 1024;
				var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

				if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

				foreach (var file in dto.UploadedFiles)
				{
					if (file.Length > maxFileSize) throw new Exception($"File '{file.FileName}' exceeds the 5MB size limit.");

					var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
					if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
						throw new Exception($"File '{file.FileName}' is invalid. Only PDF files are allowed.");

					if (file.ContentType.ToLower() != "application/pdf")
						throw new Exception($"The content of file '{file.FileName}' is not a valid PDF.");

					var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
					var filePathString = Path.Combine(uploadFolder, uniqueFileName);

					using (var fileStream = new FileStream(filePathString, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}

					var ideaDocument = new IdeaDocument { Id = Guid.NewGuid(), IdeaId = idea.Id, OriginalFileName = file.FileName, StoredPath = filePathString };
					await _context.IdeaDocuments.AddAsync(ideaDocument);
				}
			}

			await _context.SaveChangesAsync();

			// Background Email Task
			var authorName = dto.IsAnonymous ? "An anonymous employee" : ideaUser.Name;
			var ideaText = idea.Title;
			var deptId = departmentId;

			_ = Task.Run(async () =>
			{
				try
				{
					using var scope = _scopeFactory.CreateScope();
					var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdeaUser>>();
					var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

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
					}
				}
				catch (Exception ex) { Console.WriteLine($"[EMAIL ERROR]: {ex.Message}"); }
			});

			return idea.Id;
		}

		// GET ALL IDEAS (PAGINATION, SORTING, FILTERING)

		public async Task<PagedResult<IdeaInfoDto>> GetIdeasPagedAsync(IdeaQueryParameters parameters, string userId, bool isManager)
		{
			var user = await _userManager.FindByIdAsync(userId);
			var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

			bool isAdminOrQAManager = roles.Contains(RoleConstants.Administrator) || roles.Contains(RoleConstants.QAManager);
			bool isQACoordinator = roles.Contains(RoleConstants.QACoordinator);

			var query = _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
					.ThenInclude(c => c.User)
				.Include(i => i.IdeaReactions)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
			{
				var search = parameters.SearchTerm.ToLower().Trim();
				query = query.Where(i => i.Title.ToLower().Contains(search) || i.Description.ToLower().Contains(search));
			}

			if (!string.IsNullOrWhiteSpace(parameters.IdeaKeyword))
			{
				var ideaKeyword = parameters.IdeaKeyword.ToLower().Trim();
				query = query.Where(i => i.Title.ToLower().Contains(ideaKeyword) || i.Description.ToLower().Contains(ideaKeyword));
			}

			if (!string.IsNullOrWhiteSpace(parameters.CategoryKeyword))
			{
				var categoryKeyword = parameters.CategoryKeyword.ToLower().Trim();
				query = query.Where(i => i.Category != null && i.Category.Name.ToLower().Contains(categoryKeyword));
			}

			if (!string.IsNullOrWhiteSpace(parameters.DepartmentKeyword))
			{
				var departmentKeyword = parameters.DepartmentKeyword.ToLower().Trim();
				query = query.Where(i => i.Department != null && i.Department.Name.ToLower().Contains(departmentKeyword));
			}

			if (!string.IsNullOrWhiteSpace(parameters.SubmissionKeyword))
			{
				var submissionKeyword = parameters.SubmissionKeyword.ToLower().Trim();
				query = query.Where(i => i.Submission != null && i.Submission.Name.ToLower().Contains(submissionKeyword));
			}

			if (parameters.IdeaId.HasValue && parameters.IdeaId.Value != Guid.Empty)
				query = query.Where(i => i.Id == parameters.IdeaId.Value);

			if (parameters.CategoryId.HasValue && parameters.CategoryId.Value != Guid.Empty)
				query = query.Where(i => i.CategoryId == parameters.CategoryId.Value);

			// Lọc theo Submission (nếu có)
			if (parameters.SubmissionId.HasValue && parameters.SubmissionId.Value != Guid.Empty)
				query = query.Where(i => i.SubmissionId == parameters.SubmissionId.Value);

			// 2. XỬ LÝ QUYỀN VÀ LỌC TRẠNG THÁI / PHÒNG BAN CHUẨN XÁC
			if (isAdminOrQAManager)
			{
				// Admin & QA Manager: Thấy TẤT CẢ phòng ban, TẤT CẢ trạng thái (tuỳ chọn filter)
				if (parameters.DepartmentId.HasValue && parameters.DepartmentId.Value != Guid.Empty)
					query = query.Where(i => i.DepartmentId == parameters.DepartmentId.Value);

				if (parameters.ReviewStatus.HasValue)
					query = query.Where(i => i.ReviewStatus == parameters.ReviewStatus.Value);
			}
			else if (isQACoordinator)
			{
				// QA Coordinator: CHỈ THẤY phòng ban của mình (dept only), nhưng thấy TẤT CẢ trạng thái để duyệt
				query = query.Where(i => i.DepartmentId == user.DepartmentId);

				if (parameters.ReviewStatus.HasValue)
					query = query.Where(i => i.ReviewStatus == parameters.ReviewStatus.Value);
			}
			else
			{
			
				query = query.Where(i => i.ReviewStatus == ReviewStatus.APPROVED);

				if (parameters.DepartmentId.HasValue && parameters.DepartmentId.Value != Guid.Empty)
					query = query.Where(i => i.DepartmentId == parameters.DepartmentId.Value);
			}

			// 3. SORTING
			switch (parameters.SortBy?.ToLower())
			{
				case "popular":
					query = query.OrderByDescending(i =>
						(i.IdeaReactions != null ? i.IdeaReactions.Count(r => r.Reaction == "thumbs_up") : 0) -
						(i.IdeaReactions != null ? i.IdeaReactions.Count(r => r.Reaction == "thumbs_down") : 0));
					break;
				case "viewed":
					query = query.OrderByDescending(i => i.ViewCount);
					break;
				case "latest_comments":
					query = query.OrderByDescending(i => (i.Comments != null && i.Comments.Any()) ? i.Comments.Max(c => c.CreatedAt) : DateTime.MinValue);
					break;
				case "latest":
				default:
					query = query.OrderByDescending(i => i.CreatedAt);
					break;
			}

			// 4. PAGINATION
			int totalCount = await query.CountAsync();
			var ideas = await query
				.Skip((parameters.PageNumber - 1) * parameters.PageSize)
				.Take(parameters.PageSize)
				.ToListAsync();

			var resultItems = new List<IdeaInfoDto>();
			foreach (var idea in ideas)
			{
				resultItems.Add(await MapToDtoAsync(idea));
			}

			return new PagedResult<IdeaInfoDto>
			{
				Items = resultItems,
				TotalCount = totalCount,
				PageNumber = parameters.PageNumber,
				PageSize = parameters.PageSize
			};
		}

		// GET IDEAS BY DEPARTMENT
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByDepartmentAsync(string userId)
		{
			var ideaUser = await _userManager.FindByIdAsync(userId);
			if (ideaUser == null || ideaUser.DepartmentId == null) return Enumerable.Empty<IdeaInfoDto>();

			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
				.Include(i => i.IdeaReactions) 
				.Where(i => i.DepartmentId == ideaUser.DepartmentId.Value)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			var result = new List<IdeaInfoDto>();
			foreach (var i in ideas) result.Add(await MapToDtoAsync(i));
			return result;
		}

		// GET IDEAS BY STAFF
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId)
		{
			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
				.Include(i => i.IdeaReactions) // Đã thêm Include
				.Where(i => i.UserId == userId)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			var result = new List<IdeaInfoDto>();
			foreach (var i in ideas) result.Add(await MapToDtoAsync(i));
			return result;
		}

		// REVIEW IDEA
		public async Task<bool> ReviewIdeaAsync(Guid ideaId, ReviewIdeaDto reviewDto, string reviewerId)
		{
			var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id == ideaId);
			if (idea == null) return false;

			// 1. Check role constant (DEPARTMENT)
			var reviewer = await _userManager.FindByIdAsync(reviewerId);
			if (reviewer == null) return false;

			// Kiểm tra xem người duyệt có phải là Admin hoặc QA Manager
			var roles = await _userManager.GetRolesAsync(reviewer);
			bool isGlobalReviewer = roles.Contains(RoleConstants.Administrator) || roles.Contains(RoleConstants.QAManager);

			// Nếu không phải quyền Global (tức là QA Coordinator), bắt buộc phải cùng phòng ban với Idea
			if (!isGlobalReviewer)
			{
				if (reviewer.DepartmentId == null || reviewer.DepartmentId != idea.DepartmentId)
				{
					throw new UnauthorizedAccessException("You can only review ideas submitted by staff within your own department.");
				}
			}

			idea.ReviewStatus = reviewDto.Status;

			idea.UpdatedAt = DateTime.UtcNow;

			_context.Ideas.Update(idea);
			var isUpdated = await _context.SaveChangesAsync() > 0;

			if (isUpdated && (reviewDto.Status == ReviewStatus.APPROVED || reviewDto.Status == ReviewStatus.REJECTED))
			{
				await SendReviewNotificationEmailAsync(idea, reviewDto.Status, reviewDto.Note);
			}

			return isUpdated;
		}

		private async Task SendReviewNotificationEmailAsync(Idea idea, ReviewStatus reviewStatus, string? note)
		{
			var submitter = await _userManager.FindByIdAsync(idea.UserId);
			if (submitter == null || string.IsNullOrWhiteSpace(submitter.Email))
			{
				return;
			}

			string subject = string.Empty;
			string body = string.Empty;

			if (reviewStatus == ReviewStatus.APPROVED)
			{
				subject = $"[Thông báo] Ý tưởng '{idea.Title}' đã được PHÊ DUYỆT";
				body = $@"
					<h3>Chúc mừng bạn!</h3>
					<p>Ý tưởng <b>{idea.Title}</b> của bạn đã được ban quản trị phê duyệt.</p>
					<p>Cảm ơn bạn đã đóng góp cho hệ thống!</p>";
			}
			else if (reviewStatus == ReviewStatus.REJECTED)
			{
				subject = $"[Thông báo] Ý tưởng '{idea.Title}' đã bị TỪ CHỐI";
				body = $@"
					<h3>Rất tiếc!</h3>
					<p>Ý tưởng <b>{idea.Title}</b> của bạn không được phê duyệt vào lúc này.</p>
					{(string.IsNullOrWhiteSpace(note) ? "" : $"<p><b>Lý do:</b> {note}</p>")}
					<p>Đừng nản lòng, hãy tiếp tục đóng góp những ý tưởng khác nhé.</p>";
			}

			try
			{
				await _emailService.SendEmailAsync(submitter.Email, subject, body);
			}
			catch
			{
			}
		}

		// GET IDEAS WITHOUT COMMENT
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync()
		{
			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Include(i => i.Department)
				.Include(i => i.Submission)
				.Include(i => i.Comments)
				.Include(i => i.IdeaReactions)
				.Where(i => i.Comments.Count == 0)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			var result = new List<IdeaInfoDto>();
			foreach (var i in ideas) result.Add(await MapToDtoAsync(i));
			return result;
		}

		// INTERACT & VOTE
		public async Task<bool> VoteIdeaAsync(Guid ideaId, string userId, bool isThumbsUp)
		{
			var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id == ideaId);
			if (idea == null) return false;

			var existingReaction = await _context.IdeaReactions.FirstOrDefaultAsync(r => r.IdeaId == idea.Id && r.UserId == userId);
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

		// GET TITLES BY USER (COMMA-SEPARATED)
		public async Task<string?> GetIdeasByUserAsync(string userId)
		{
			var ideas = await _context.Ideas
				.Where(i => i.UserId == userId)
				.Select(i => i.Title)
				.ToListAsync();

			return ideas.Count == 0 ? null : string.Join(", ", ideas);
		}

		// CREATE COMMENT & EMAIL NOTIFICATION
		public async Task<CommentDto?> CreateCommentAsync(CommentCreateDto dto, string userId)
		{
			var commentUser = await _userManager.FindByIdAsync(userId);
			if (commentUser == null) return null; // Sửa thành return null

			var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id == dto.IdeaId);
			if (idea == null) return null; // Sửa thành return null

			var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == idea.SubmissionId);
			if (submission == null || DateTime.UtcNow.Date > submission.FinalClosureDate) return null; // Sửa thành return null

			var comment = new Comment
			{
				Id = Guid.NewGuid(),
				IdeaId = dto.IdeaId,
				UserId = userId,
				Content = dto.Content,
				IsAnonymous = dto.IsAnonymous,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Comments.AddAsync(comment);
			await _context.SaveChangesAsync();

			var newCommentDto = new CommentDto
			{
				Id = comment.Id,
				Content = comment.Content,
				CreatedDate = comment.CreatedAt,
				IsAnonymous = comment.IsAnonymous,
				AuthorName = comment.IsAnonymous ? "Anonymous" : commentUser.Name
			};

			var commenterName = dto.IsAnonymous ? "An anonymous employee" : commentUser.Name;
			var authorUser = await _userManager.FindByIdAsync(idea.UserId);
			var isSelfCommenting = (idea.UserId == userId);

			// THÔNG BÁO EMAIL (Chạy ngầm)
			if (authorUser != null && !string.IsNullOrEmpty(authorUser.Email) && !isSelfCommenting)
			{
				var authorEmail = authorUser.Email;
				var authorName = authorUser.Name;
				var ideaTitle = idea.Title;
				var commentText = comment.Content;

				_ = Task.Run(async () =>
				{
					try
					{
						using var scope = _scopeFactory.CreateScope();
						var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

						string subject = "🔔 [Idea System] Someone commented on your idea!";
						string body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eaeaea; border-radius: 8px;'>
                        <h3 style='color: #2c3e50;'>Hello {authorName},</h3>
                        <p><strong>{commenterName}</strong> has just left a new comment on your idea.</p>
                        <div style='background-color: #f1f8ff; padding: 10px; border-left: 4px solid #007bff; margin: 15px 0;'>
                            <p style='margin: 0; color: #555; font-size: 13px;'>Your Idea:</p>
                            <i>""{ideaTitle}""</i>
                        </div>
                        <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #28a745; margin: 15px 0;'>
                            <p style='margin: 0; color: #555; font-size: 13px;'>New Comment:</p>
                            <strong>""{commentText}""</strong>
                        </div>
                        <p>Log in to the system to reply and keep the discussion going!</p>
                    </div>";

						await emailService.SendEmailAsync(authorEmail, subject, body);
					}
					catch (Exception ex) { Console.WriteLine($"[EMAIL ERROR]: {ex.Message}"); }
				});
			}
			return newCommentDto;
		}

		// GET IDEA DETAIL 

		public async Task<IdeaInfoDto?> GetIdeaDetailAsync(Guid id, string userId)
		{
			var idea = await _context.Ideas
		.Include(i => i.Category)
		.Include(i => i.Department)
		.Include(i => i.Submission)
		.Include(i => i.Comments)
		.ThenInclude(c => c.User) 
		.Include(i => i.IdeaReactions)
		.FirstOrDefaultAsync(i => i.Id == id);

			if (idea == null)
			{
				return null;
			}

			var hasViewed = await _context.IdeaViews
								  .AnyAsync(v => v.IdeaId == id && v.UserId == userId);

			if (!hasViewed)
			{
				var newViewRecord = new IdeaCollectionSystem.ApplicationCore.Entitites.IdeaView
				{
					IdeaId = id,
					UserId = userId,
					ViewedAt = DateTime.UtcNow
				};

				_context.IdeaViews.Add(newViewRecord);
				idea.ViewCount += 1;
				_context.Ideas.Update(idea);

				await _context.SaveChangesAsync();
			}

			var ideaDto = await MapToDtoAsync(idea);
			return ideaDto;
		}

		// GET MY IDEAS (PAGINATION, SORTING, FILTERING)
		public async Task<PagedResult<IdeaInfoDto>> GetMyIdeasPagedAsync(IdeaQueryParameters parameters, string userId)
		{
					var query = _context.Ideas
			.Include(i => i.Category)
			.Include(i => i.Department)
			.Include(i => i.Submission)
			.Include(i => i.Comments)
			.ThenInclude(c => c.User) 
			.Include(i => i.IdeaReactions)
			.AsQueryable();

			if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
			{
				var search = parameters.SearchTerm.ToLower().Trim();
				query = query.Where(i => i.Title.ToLower().Contains(search) || i.Description.ToLower().Contains(search));
			}

			if (!string.IsNullOrWhiteSpace(parameters.IdeaKeyword))
			{
				var ideaKeyword = parameters.IdeaKeyword.ToLower().Trim();
				query = query.Where(i => i.Title.ToLower().Contains(ideaKeyword) || i.Description.ToLower().Contains(ideaKeyword));
			}

			if (!string.IsNullOrWhiteSpace(parameters.CategoryKeyword))
			{
				var categoryKeyword = parameters.CategoryKeyword.ToLower().Trim();
				query = query.Where(i => i.Category != null && i.Category.Name.ToLower().Contains(categoryKeyword));
			}

			if (!string.IsNullOrWhiteSpace(parameters.DepartmentKeyword))
			{
				var departmentKeyword = parameters.DepartmentKeyword.ToLower().Trim();
				query = query.Where(i => i.Department != null && i.Department.Name.ToLower().Contains(departmentKeyword));
			}

			if (!string.IsNullOrWhiteSpace(parameters.SubmissionKeyword))
			{
				var submissionKeyword = parameters.SubmissionKeyword.ToLower().Trim();
				query = query.Where(i => i.Submission != null && i.Submission.Name.ToLower().Contains(submissionKeyword));
			}

			if (parameters.IdeaId.HasValue && parameters.IdeaId.Value != Guid.Empty)
			{
				query = query.Where(i => i.Id == parameters.IdeaId.Value);
			}

			if (parameters.CategoryId.HasValue && parameters.CategoryId.Value != Guid.Empty)
			{
				query = query.Where(i => i.CategoryId == parameters.CategoryId.Value);
			}

			if (parameters.SubmissionId.HasValue && parameters.SubmissionId.Value != Guid.Empty)
			{
				query = query.Where(i => i.SubmissionId == parameters.SubmissionId.Value);
			}

			if (parameters.DepartmentId.HasValue && parameters.DepartmentId.Value != Guid.Empty)
			{
				query = query.Where(i => i.DepartmentId == parameters.DepartmentId.Value);
			}

			if (parameters.ReviewStatus.HasValue)
			{
				query = query.Where(i => i.ReviewStatus == parameters.ReviewStatus.Value);
			}

			// 2. SORTING
			switch (parameters.SortBy?.ToLower())
			{
				case "popular":
					query = query.OrderByDescending(i =>
						(i.IdeaReactions != null ? i.IdeaReactions.Count(r => r.Reaction == "thumbs_up") : 0) -
						(i.IdeaReactions != null ? i.IdeaReactions.Count(r => r.Reaction == "thumbs_down") : 0));
					break;
				case "viewed":
					query = query.OrderByDescending(i => i.ViewCount);
					break;
				case "latest_comments":
					query = query.OrderByDescending(i => (i.Comments != null && i.Comments.Any()) ? i.Comments.Max(c => c.CreatedAt) : DateTime.MinValue);
					break;
				case "latest":
				default:
					query = query.OrderByDescending(i => i.CreatedAt);
					break;
			}

			// 3. PAGINATION
			int totalCount = await query.CountAsync();
			var ideas = await query
				.Skip((parameters.PageNumber - 1) * parameters.PageSize)
				.Take(parameters.PageSize)
				.ToListAsync();

			// 4. MAP TO DTO (Sử dụng hàm Helper MapToDtoAsync đã viết ở bước trước cho gọn)
			var resultItems = new List<IdeaInfoDto>();
			foreach (var idea in ideas)
			{
				resultItems.Add(await MapToDtoAsync(idea));
			}

			return new PagedResult<IdeaInfoDto>
			{
				Items = resultItems,
				TotalCount = totalCount,
				PageNumber = parameters.PageNumber,
				PageSize = parameters.PageSize
			};
		}
	}
}