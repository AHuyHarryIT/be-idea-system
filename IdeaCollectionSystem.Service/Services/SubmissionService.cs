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
	}
}