using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;

namespace IdeaCollectionSystem.Service.Services
{
	public class QAManagerService : IQAManagerService
	{
		private readonly IdeaCollectionDbContext _context;
		private readonly UserManager<IdeaUser> _userManager;

		public QAManagerService(IdeaCollectionDbContext context, UserManager<IdeaUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		//  Dashboard 
		public async Task<QaDashboardDto> GetDashboardStatsAsync()
		{
			return new QaDashboardDto
			{
				TotalIdeas = await _context.Ideas.CountAsync(),
				TotalCategories = await _context.Categories.CountAsync(),
				TotalDepartments = await _context.Departments.CountAsync(),
				TotalUsers = await _userManager.Users.CountAsync(),
				IdeasWithoutComments = await _context.Ideas.CountAsync(i => !i.Comments.Any()),
				IdeasThisMonth = await _context.Ideas
					.CountAsync(i => i.CreatedAt.Month == DateTime.UtcNow.Month
								  && i.CreatedAt.Year == DateTime.UtcNow.Year)
			};
		}

		//  CSV Export 
		public async Task<byte[]> ExportIdeasToCsvAsync()
		{
			var ideas = await _context.Ideas
				.Include(i => i.User)
				.Include(i => i.Category)
				.Include(i => i.Department)
				.ToListAsync();

			var csv = new StringBuilder();
			csv.AppendLine("IdeaID,Title,Author,Category,Department,Date,Upvotes,Downvotes,Comments");

			foreach (var i in ideas)
			{
				var upvotes = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_up");
				var downvotes = await _context.IdeaReactions.CountAsync(r => r.IdeaId == i.Id && r.Reaction == "thumbs_down");
				var comments = await _context.Comments.CountAsync(c => c.IdeaId == i.Id);
				var author = i.IsAnonymous ? "Anonymous" : (i.User?.FirstName + " " + i.User?.LastName);
				csv.AppendLine(
					$"{i.Id},\"{i.Text}\",\"{author}\",\"{i.Category?.Name}\"," +
					$"\"{i.Department?.Name}\",\"{i.CreatedAt:yyyy-MM-dd HH:mm}\",{upvotes},{downvotes},{comments}");
			}

			return Encoding.UTF8.GetBytes(csv.ToString());
		}

		//  ZIP Export 
		public async Task<byte[]> ExportDocumentsToZipAsync()
		{
			var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
			using var memoryStream = new MemoryStream();

			using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
			{
				if (Directory.Exists(uploadPath))
				{
					var files = Directory.GetFiles(uploadPath);
					foreach (var file in files)
						archive.CreateEntryFromFile(file, Path.GetFileName(file));
				}
			}

			return await Task.FromResult(memoryStream.ToArray());
		}

		//  Department Statistics 
		public async Task<IEnumerable<DepartmentStatDto>> GetDepartmentStatisticsAsync()
		{
			var totalIdeas = await _context.Ideas.CountAsync();

			return await _context.Departments
				.Select(d => new DepartmentStatDto
				{
					DepartmentName = d.Name,
					IdeaCount = d.Ideas.Count(),
					Percentage = totalIdeas > 0 ? Math.Round((double)d.Ideas.Count() / totalIdeas * 100, 1) : 0,
					ContributorCount = d.Ideas.Select(i => i.UserId).Distinct().Count()
				})
				.ToListAsync();
		}

		//  Ideas without comments 
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync()
		{
			return await _context.Ideas
				.Include(i => i.Category)
				.Where(i => !i.Comments.Any())
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(),
					Title = i.Text,
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous
				})
				.ToListAsync();
		}

		//  Submissions / Closure Dates 
		public async Task<IEnumerable<SubmissionDto>> GetAllSubmissionsAsync()
		{
			return await _context.Submissions
				.Select(s => new SubmissionDto
				{
					Id = s.Id,
					Name = s.Name,
					AcademicYear = s.AcademicYear,
					ClosureDate = s.ClousureDate,
					FinalClosureDate = s.FinalClousureDate,
					IdeaCount = s.Ideas.Count(),
					IsActive = DateTime.UtcNow <= s.ClousureDate
				})
				.OrderByDescending(s => s.AcademicYear)
				.ToListAsync();
		}

		public async Task<bool> CreateSubmissionAsync(SubmissionCreateDto dto)
		{
			var submission = new Submission
			{
				Id = Guid.NewGuid(),
				Name = dto.Name,
				AcademicYear = dto.AcademicYear,
				ClousureDate = dto.ClosureDate,
				FinalClousureDate = dto.FinalClosureDate,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Submissions.AddAsync(submission);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<bool> UpdateSubmissionAsync(Guid id, SubmissionCreateDto dto)
		{
			var submission = await _context.Submissions.FindAsync(id);
			if (submission == null) return false;

			submission.Name = dto.Name;
			submission.AcademicYear = dto.AcademicYear;
			submission.ClousureDate = dto.ClosureDate;
			submission.FinalClousureDate = dto.FinalClosureDate;
			submission.UpdatedAt = DateTime.UtcNow;

			return await _context.SaveChangesAsync() > 0;
		}

		//  Users (Admin only) 
		public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
		{
			var users = await _userManager.Users.ToListAsync();
			var result = new List<UserDto>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				result.Add(new UserDto
				{
					Id = user.Id,
					Name = user.Name,
					Email = user.Email ?? "",
					Role = roles.FirstOrDefault() ?? "No Role",
					Avatar = user.Avatar
				});
			}

			return result;
		}

		public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			var currentRoles = await _userManager.GetRolesAsync(user);
			await _userManager.RemoveFromRolesAsync(user, currentRoles);
			var result = await _userManager.AddToRoleAsync(user, newRole);
			return result.Succeeded;
		}

		public async Task<bool> DeleteUserAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			var result = await _userManager.DeleteAsync(user);
			return result.Succeeded;
		}
	}
}