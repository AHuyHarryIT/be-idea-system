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

		// Get Submission
		public async Task<PagedResult<SubmissionDto>> GetSubmissionsPagedAsync(PaginationFilter filter)
		{
			var query = _context.Submissions.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
			{
				var search = filter.SearchTerm.ToLower().Trim();
				query = query.Where(s => s.Name.ToLower().Contains(search));
			}

			var totalCount = await query.CountAsync();

	
			var submissions = await query
				.OrderByDescending(s => s.CreatedAt)
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			var result = submissions.Select(s => new SubmissionDto
			{
				Id = s.Id,
				Name = s.Name,
				Description = s.Description,
				ClosureDate = s.ClosureDate,
				FinalClosureDate = s.FinalClosureDate
			}).ToList();

			return new PagedResult<SubmissionDto>(result, totalCount, filter.PageNumber, filter.PageSize);
		}

		// Get all submisssions
		public async Task<IEnumerable<SubmissionDto>> GetAllSubmissionsAsync()
		{
			return await _context.Submissions

				.OrderByDescending(s => s.CreatedAt)
				.Select(s => new SubmissionDto
				{
					Id = s.Id,
					Name = s.Name,
					Description = s.Description,
					AcademicYear = s.AcademicYear, 
					ClosureDate = s.ClosureDate,
					FinalClosureDate = s.FinalClosureDate,
					IdeaCount = s.Ideas.Count(),
					IsActive = DateTime.UtcNow <= s.ClosureDate
				})
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
				ClosureDate = dto.ClosureDate,
				FinalClosureDate = dto.FinalClosureDate,
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
			if (submission == null)
				throw new Exception("The submission does not exist."); 

			if (submission.ClosureDate < DateTime.UtcNow)
			{
				throw new Exception("The closure date has already passed. You cannot edit this submission anymore.");
			}

			if (dto.ClosureDate >= dto.FinalClosureDate)
			{
				throw new Exception("The Closure Date must be earlier than the Final Closure Date.");
			}

			submission.Name = dto.Name;
			submission.Description = dto.Description;
			submission.AcademicYear = dto.AcademicYear;
			submission.ClosureDate = dto.ClosureDate;
			submission.FinalClosureDate = dto.FinalClosureDate;
			submission.UpdatedAt = DateTime.UtcNow;

			return await _context.SaveChangesAsync() > 0;
		}

		// DELETE submission
		public async Task<(bool Success, string Message)> DeleteSubmissionAsync(Guid id)
		{
	
			var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id);
			if (submission == null)
			{
				return (false, "The submission does not exist.");
			}

			var hasIdeas = await _context.Ideas.AnyAsync(i => i.SubmissionId == id);
			if (hasIdeas)
			{
				// Nếu có rồi ->  cấm xóa!
				return (false, "Cannot delete this submission because employees have already submitted ideas to it. Please close it instead.");
			}

			// 3. Nếu chưa có Idea nào -> An toàn để xóa
			_context.Submissions.Remove(submission);
			await _context.SaveChangesAsync();

			return (true, "The submission has been deleted successfully.");
		}

	}
}