using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services
{
	public class SubmissionService : ISubmissionService
	{
		private readonly IdeaCollectionDbContext _context;

		public SubmissionService(IdeaCollectionDbContext context)
		{
			_context = context;
		}

		// Get all submisssions
		public async Task<IEnumerable<SubmissionDto>> GetAllSubmissionsAsync()
		{
			return await _context.Submissions
				.Select(s => new SubmissionDto
				{
					Id = s.Id,
					Name = s.Name,
					Description = s.Description,
					//AcademicYear = s.AcademicYear,
					ClosureDate = s.ClousureDate,
					FinalClosureDate = s.FinalClousureDate,
					IdeaCount = s.Ideas.Count(),
					IsActive = DateTime.UtcNow <= s.ClousureDate
				})
				.OrderByDescending(s => s.AcademicYear)
				.ToListAsync();
		}

		// Create submission
		public async Task<bool> CreateSubmissionAsync(SubmissionCreateDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Name))
			{
				return false;
			}

			if (dto.ClosureDate < DateTime.UtcNow)
			{
				return false;
			}

			if (dto.FinalClosureDate <= dto.ClosureDate)
			{
				return false;
			}

			var submission = new Submission
			{
				Id = Guid.NewGuid(),
				Name = dto.Name,
				Description = dto.Description,
				AcademicYear = dto.AcademicYear,
				ClousureDate = dto.ClosureDate,
				FinalClousureDate = dto.FinalClosureDate,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _context.Submissions.AddAsync(submission);
			return await _context.SaveChangesAsync() > 0;
		}

		// update submission
		public async Task<bool> UpdateSubmissionAsync(Guid id, SubmissionCreateDto dto)
		{
			var submission = await _context.Submissions.FindAsync(id);
			if (submission == null) return false;

			submission.Name = dto.Name;
			submission.Description = dto.Description;
			submission.AcademicYear = dto.AcademicYear;
			submission.ClousureDate = dto.ClosureDate;
			submission.FinalClousureDate = dto.FinalClosureDate;
			submission.UpdatedAt = DateTime.UtcNow;

			return await _context.SaveChangesAsync() > 0;
		}
		public async Task<(bool Success, string Message)> DeleteSubmissionAsync(Guid id)
		{
			// 1. Tìm Submission trong DB
			var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id);
			if (submission == null)
			{
				return (false, "The submission does not exist.");
			}

			var hasIdeas = await _context.Ideas.AnyAsync(i => i.SubmissionId == id);
			if (hasIdeas)
			{
				// Nếu có rồi -> Rút thẻ đỏ, cấm xóa!
				return (false, "Cannot delete this submission because employees have already submitted ideas to it. Please close it instead.");
			}

			// 3. Nếu chưa có Idea nào -> An toàn để xóa
			_context.Submissions.Remove(submission);
			await _context.SaveChangesAsync();

			return (true, "The submission has been deleted successfully.");
		}

	}
}