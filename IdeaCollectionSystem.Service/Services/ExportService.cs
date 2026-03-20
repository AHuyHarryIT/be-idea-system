using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;

namespace IdeaCollectionSystem.Service.Services
{
	public class ExportService : IExportService
	{
		private readonly IdeaCollectionDbContext _context;
		private readonly UserManager<IdeaUser> _userManager;

		public ExportService(IdeaCollectionDbContext context, UserManager<IdeaUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<byte[]> ExportIdeasToCsvAsync()
		{
			var ideas = await _context.Ideas
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

				string author;
				if (i.IsAnonymous)
				{
					author = "Anonymous";
				}
				else
				{
					var user = await _userManager.FindByIdAsync(i.UserId);
					author = user?.Name ?? user?.Email ?? "Unknown";
				}

				csv.AppendLine(
					$"{i.Id},\"{i.Title}\",\"{author}\",\"{i.Category?.Name}\"," +
					$"\"{i.Department?.Name}\",\"{i.CreatedAt:yyyy-MM-dd HH:mm}\",{upvotes},{downvotes},{comments}");
			}

			return Encoding.UTF8.GetBytes(csv.ToString());
		}

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
	}
}