using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

			var baseUrl = "https://ideacollectionsystemapi20260313215839-brd4bqdwfbgeg7fj.southeastasia-01.azurewebsites.net";

			var dto = new IdeaInfoDto
			{
				Id = idea.Id,
				Title = idea.Title,
				Description = idea.Description,
				SubmissionId = idea.SubmissionId,
				SubmissionName = idea.Submission?.Name ?? "Unknown Submission",
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

				Documents = idea.IdeaDocuments?.Select(d => new DocumentDto
				{
					Id = d.Id,
					FileName = d.OriginalFileName,

					FileUrl = baseUrl + "/uploads/" + System.IO.Path.GetFileName(d.StoredPath)

				}).ToList() ?? new List<DocumentDto>(),
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

			return DateTime.UtcNow > latestSubmission.ClosureDate;
		}

		// Check Final Closure date
		public async Task<bool> IsFinalClosureDatePassedAsync(Guid ideaId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Submission)
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea?.Submission == null) return true;

			return DateTime.UtcNow > idea.Submission.FinalClosureDate;
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

			if (submission == null || DateTime.UtcNow > submission.ClosureDate) return null;

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
				var maxFileSize = 5 * 1024 * 1024; // 5MB

				var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

				if (!Directory.Exists(uploadFolder))
				{
					Directory.CreateDirectory(uploadFolder);
				}

				foreach (var file in dto.UploadedFiles)
				{
					if (file.Length > maxFileSize)
						throw new Exception($"File '{file.FileName}' exceeds the 5MB size limit.");

					var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
					if (string.IsNullOrEmpty(extension) ||
						!allowedExtensions.Contains(extension) ||
						file.ContentType.ToLower() != "application/pdf")
					{
						throw new Exception($"File '{file.FileName}' is invalid. Only PDF files are allowed.");
					}

					var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

			
					var physicalPath = Path.Combine(uploadFolder, uniqueFileName);

	
					var relativeHttpPath = $"/uploads/{uniqueFileName}";

					using (var fileStream = new FileStream(physicalPath, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}

					var ideaDocument = new IdeaDocument
					{
						Id = Guid.NewGuid(),
						IdeaId = idea.Id,
						OriginalFileName = file.FileName,
						StoredPath = relativeHttpPath
					};

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

	

		// UPDATE IDEA
		public async Task<bool> UpdateIdeaAsync(Guid ideaId, IdeaUpdateDto dto, string userId)
		{
			var idea = await _context.Ideas
				.Include(i => i.Submission)
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea == null)
				throw new Exception("Idea not found.");

			if (idea.UserId != userId)
				throw new UnauthorizedAccessException("You can only edit your own idea.");

			if (idea.Submission == null || DateTime.UtcNow > idea.Submission.ClosureDate)
				throw new Exception("The submission closure date has passed. You cannot edit this idea anymore.");


			idea.Title = dto.Title;
			idea.Description = dto.Description;
			idea.CategoryId = dto.CategoryId;
			idea.IsAnonymous = dto.IsAnonymous;
			idea.UpdatedAt = DateTime.UtcNow;

			idea.ReviewStatus = ReviewStatus.PENDING;

			_context.Ideas.Update(idea);

			if (dto.UploadedFiles != null && dto.UploadedFiles.Any())
			{
				var allowedExtensions = new[] { ".pdf" };
				var maxFileSize = 5 * 1024 * 1024; 

				var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

				if (!Directory.Exists(uploadFolder))
				{
					Directory.CreateDirectory(uploadFolder);
				}

				foreach (var file in dto.UploadedFiles)
				{
					if (file.Length > maxFileSize)
						throw new Exception($"File '{file.FileName}' exceeds the 5MB size limit.");

					var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
					if (string.IsNullOrEmpty(extension) ||
						!allowedExtensions.Contains(extension) ||
						file.ContentType.ToLower() != "application/pdf")
					{
						throw new Exception($"File '{file.FileName}' is invalid. Only PDF files are allowed.");
					}

					var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
					var physicalPath = Path.Combine(uploadFolder, uniqueFileName);
					var relativeHttpPath = $"/uploads/{uniqueFileName}";

					using (var fileStream = new FileStream(physicalPath, FileMode.Create))
					{
						await file.CopyToAsync(fileStream);
					}

					var ideaDocument = new IdeaDocument
					{
						Id = Guid.NewGuid(),
						IdeaId = idea.Id,
						OriginalFileName = file.FileName,
						StoredPath = relativeHttpPath
					};

					await _context.IdeaDocuments.AddAsync(ideaDocument);
				}
			}

			await _context.SaveChangesAsync();

			// 6. Background Task
			var ideaUser = await _userManager.FindByIdAsync(userId);
			var authorName = dto.IsAnonymous ? "An anonymous employee" : (ideaUser?.Name ?? "An employee");
			var ideaText = idea.Title;
			var deptId = idea.DepartmentId;

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
						string subject = "💡 [Idea System] An idea has been updated and requires re-review!";
						string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eaeaea; border-radius: 8px;'>
                    <h3 style='color: #2c3e50;'>Hello {deptCoordinator.Name},</h3>
                    <p><strong>{authorName}</strong> from your Department has just updated their idea.</p>
                    <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #f39c12; margin: 15px 0;'>
                        <i>""{ideaText}""</i>
                    </div>
                    <p>The status has been reset to <strong>PENDING</strong>. Please log in to the system to review the changes and newly attached files.</p>
                    <br/>
                    <p style='font-size: 12px; color: #888;'>This is an automated message, please do not reply to this email.</p>
                </div>";

						await emailService.SendEmailAsync(deptCoordinator.Email, subject, body);
					}
				}
				catch (Exception ex) { Console.WriteLine($"[EMAIL ERROR]: {ex.Message}"); }
			});

			return true;
		}

		// DELETE IDEA (Dành cho Admin, QAM, QAC)
		public async Task<bool> DeleteIdeaAsync(Guid ideaId)
		{
	
			var idea = await _context.Ideas
				.Include(i => i.IdeaDocuments)
				.Include(i => i.Submission) 
				.FirstOrDefaultAsync(i => i.Id == ideaId);

			if (idea == null)
				throw new Exception("Idea not found.");

			if (idea.Submission != null && DateTime.UtcNow > idea.Submission.FinalClosureDate)
			{
				throw new Exception("The final closure date has passed. This idea is permanently archived and cannot be deleted.");
			}

			// 3. Dọn rác vật lý ổ cứng
			if (idea.IdeaDocuments != null && idea.IdeaDocuments.Any())
			{

				var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

				foreach (var doc in idea.IdeaDocuments)
				{
			
					var fileName = System.IO.Path.GetFileName(doc.StoredPath);

					var physicalPath = Path.Combine(uploadFolder, fileName);

					if (File.Exists(physicalPath))
					{
						File.Delete(physicalPath);
					}
				}
			}

			_context.Ideas.Remove(idea);
			await _context.SaveChangesAsync();

			return true;
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
				.Include(i => i.IdeaDocuments)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
			{
				var search = parameters.SearchTerm.ToLower().Trim();
				query = query.Where(i => i.Title.ToLower().Contains(search) || i.Description.ToLower().Contains(search));
			}

			// Lọc theo Submission 
			if (!parameters.SubmissionId.HasValue || parameters.SubmissionId.Value == Guid.Empty)
			{
				var latestSubmission = await _context.Submissions
					.OrderByDescending(s => s.ClosureDate)
					.FirstOrDefaultAsync();

				if (latestSubmission != null)
				{
					parameters.SubmissionId = latestSubmission.Id;
				}
			}

			if (parameters.SubmissionId.HasValue && parameters.SubmissionId.Value != Guid.Empty)
			{
				query = query.Where(i => i.SubmissionId == parameters.SubmissionId.Value);
			}


			if (parameters.SubmissionId.HasValue && parameters.SubmissionId.Value != Guid.Empty)
				query = query.Where(i => i.SubmissionId == parameters.SubmissionId.Value);

			if (parameters.CategoryId.HasValue && parameters.CategoryId.Value != Guid.Empty)
			{
				query = query.Where(i => i.CategoryId == parameters.CategoryId.Value);
			}

			// 2. XỬ LÝ QUYỀN VÀ LỌC TRẠNG THÁI 
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
				.Include(i => i.IdeaReactions)
				.Where(i => i.UserId == userId)
				.OrderByDescending(i => i.CreatedAt)
				.ToListAsync();

			var result = new List<IdeaInfoDto>();
			foreach (var i in ideas) result.Add(await MapToDtoAsync(i));
			return result;
		}


		// REVIEW IDEA (APPROVE/REJECT) + EMAIL NOTIFICATION
		public async Task<bool> ReviewIdeaAsync(Guid ideaId, ReviewIdeaDto reviewDto, string reviewerId)
		{
			var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id == ideaId);
			if (idea == null) return false;

			// 1. Check role constant (DEPARTMENT)
			var reviewer = await _userManager.FindByIdAsync(reviewerId);
			if (reviewer == null) return false;

			// Kiểm tra role được phép duyệt
			var roles = await _userManager.GetRolesAsync(reviewer);
			bool isQACoordinator = roles.Contains(RoleConstants.QACoordinator);
			bool isGlobalReviewer = roles.Contains(RoleConstants.Administrator) || roles.Contains(RoleConstants.QAManager);

			if (!isGlobalReviewer && !isQACoordinator)
			{
				throw new UnauthorizedAccessException("Only Administrator, QA Manager, or QA Coordinator can review ideas.");
			}

			// Nếu không phải quyền Global (tức là QA Coordinator), bắt buộc phải cùng phòng ban với Idea
			if (isQACoordinator && !isGlobalReviewer)
			{
				if (reviewer.DepartmentId == null || reviewer.DepartmentId != idea.DepartmentId)
				{
					throw new UnauthorizedAccessException("You can only review ideas submitted by staff within your own department.");
				}
			}

			idea.ReviewStatus = reviewDto.Status;

			idea.UpdatedAt = DateTime.UtcNow;

			_context.Ideas.Update(idea);
			var isSaved = await _context.SaveChangesAsync() > 0;
			if (!isSaved) return false;

			if (reviewDto.Status == ReviewStatus.APPROVED || reviewDto.Status == ReviewStatus.REJECTED)
			{
				var author = await _userManager.FindByIdAsync(idea.UserId);
				if (author != null && !string.IsNullOrWhiteSpace(author.Email))
				{
					var authorEmail = author.Email;
					var reviewStatus = reviewDto.Status;
					var note = reviewDto.Note;

					string subject = string.Empty;
					string body = string.Empty;

					if (reviewStatus == ReviewStatus.APPROVED)
					{
						subject = $"[Notification] Your idea '{idea.Title}' has been APPROVED";
						body = $@"
					<h3>Congratulations!</h3>
					<p>Your idea <b>{idea.Title}</b> has been approved by the review committee.</p>
					<p>Thank you for your contribution to the system!</p>";
					}
					else if (reviewStatus == ReviewStatus.REJECTED)
					{
						subject = $"[Notification] Your idea '{idea.Title}' has been REJECTED";
						body = $@"
					<h3>We're sorry!</h3>
					<p>Unfortunately, your idea <b>{idea.Title}</b> has not been approved at this time.</p>
					{(string.IsNullOrWhiteSpace(note) ? "" : $"<p><b>Reason:</b> {note}</p>")}
					<p>Don't be discouraged, we look forward to your future contributions.</p>";
					}

					_ = Task.Run(async () =>
					{
						try
						{
							using var scope = _scopeFactory.CreateScope();
							var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

							await emailService.SendEmailAsync(authorEmail, subject, body);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"[EMAIL ERROR]: {ex.Message}");
						}
					});
				}
			}

			return true;
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
			if (commentUser == null) return null; 

			var idea = await _context.Ideas.FirstOrDefaultAsync(i => i.Id == dto.IdeaId);
			if (idea == null) return null; 

			var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == idea.SubmissionId);
			if (submission == null || DateTime.UtcNow > submission.FinalClosureDate) return null; 

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
		.Include(i => i.IdeaDocuments)
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
				.Include(i => i.IdeaDocuments)
				.AsQueryable();
			query = query.Where(i => i.UserId == userId);

			// 1. FILTERING
			if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
			{
				var search = parameters.SearchTerm.ToLower().Trim();
				query = query.Where(i => i.Title.ToLower().Contains(search) || i.Description.ToLower().Contains(search));
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

			// 4. MAP TO DTO
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