using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;

public class QAManagerService : IQAManagerService
{
	private readonly IdeaCollectionDbContext _context;

	public QAManagerService(IdeaCollectionDbContext context)
	{
		_context = context;
	}


	// Dashboard

	public async Task<QADashboardDto> GetDashboardStatsAsync()
	{
		return new QADashboardDto
		{
			TotalIdeas = await _context.Ideas.CountAsync(),
			TotalCategories = await _context.Categories.CountAsync(),
			TotalDepartments = await _context.Departments.CountAsync(),
			IdeasWithoutComments = await _context.Ideas
				.CountAsync(i => !i.Comments.Any())
		};
	}


	// CSV Export
	public async Task<byte[]> ExportIdeasToCsvAsync()
	{
		var ideas = await _context.Ideas
			.Include(i => i.User)
			.Include(i => i.Category)
			.ToListAsync();

		var csv = new StringBuilder();
		csv.AppendLine("IdeaID,Title,Author,Category,Date,Upvotes,Downvotes");

		//foreach (var i in ideas)
		//{
		//	var upvotes = await _context.Votes
		//		.CountAsync(v => v.IdeaId == i.Id && v.IsUpvote);

		//	var downvotes = await _context.Votes
		//		.CountAsync(v => v.IdeaId == i.Id && !v.IsUpvote);

		//	csv.AppendLine(
		//		$"{i.Id},\"{i.Text}\",\"{i.User?.UserName}\",\"{i.Category?.Name}\"," +
		//		$"\"{i.CreatedAt:yyyy-MM-dd HH:mm}\",{upvotes},{downvotes}");
		//}

		return Encoding.UTF8.GetBytes(csv.ToString());
	}


	// ZIP Export 
	public async Task<byte[]> ExportDocumentsToZipAsync(string uploadPath)
	{
		using var memoryStream = new MemoryStream();

		using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
		{
			if (Directory.Exists(uploadPath))
			{
				var files = Directory.GetFiles(uploadPath);

				foreach (var file in files)
				{
					archive.CreateEntryFromFile(file, Path.GetFileName(file));
				}
			}
		}

		return await Task.FromResult(memoryStream.ToArray());
	}
	public async Task<byte[]> ExportDocumentsToZipAsync()
	{
		
		throw new NotImplementedException("ExportDocumentsToZipAsync() without parameters is not implemented. Use ExportDocumentsToZipAsync(string uploadPath) instead.");
	}


	// Department Statistics

	public async Task<IEnumerable<DepartmentStatDto>> GetDepartmentStatisticsAsync()
	{
		var totalIdeas = await _context.Ideas.CountAsync();

		return await _context.Departments
			.Select(d => new DepartmentStatDto
			{
				DepartmentName = d.Name,
				IdeaCount = d.Ideas.Count(),
				Percentage = totalIdeas > 0
					? (double)d.Ideas.Count() / totalIdeas * 100
					: 0,
				ContributorCount = d.Ideas
					.Select(i => i.UserId)
					.Distinct()
					.Count()
			})
			.ToListAsync();
	}


	// Ideas without comments
	public async Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync()
	{
		return await _context.Ideas
			.Include(i => i.Category)
			.Where(i => !i.Comments.Any())
			.Select(i => new IdeaInfoDto
			{
				Id = i.Id.GetHashCode(), 
				Title = i.Text,
				CategoryName = i.Category.Name,
				CreatedDate = i.CreatedAt,
				IsAnonymous = i.IsAnonymous
			})
			.ToListAsync();
	}
}