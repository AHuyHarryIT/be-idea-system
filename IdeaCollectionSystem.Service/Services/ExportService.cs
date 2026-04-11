using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using System.Net.Http;

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

		// Export CSV (TẤT CẢ)
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

				// Escape chuỗi để tránh lỗi khi tiêu đề hoặc tên phòng ban có chứa dấu phẩy
				var title = i.Title?.Replace("\"", "\"\"");
				var category = i.Category?.Name?.Replace("\"", "\"\"");
				var department = i.Department?.Name?.Replace("\"", "\"\"");

				csv.AppendLine(
					$"{i.Id},\"{title}\",\"{author}\",\"{category}\",\"{department}\",\"{i.CreatedAt:yyyy-MM-dd HH:mm}\",{upvotes},{downvotes},{comments}");
			}

			return Encoding.UTF8.GetBytes(csv.ToString());
		}

		// Export ZIP (TẤT CẢ) - Tải file qua URL
		public async Task<byte[]> ExportDocumentsToZipAsync()
		{
			var documents = await _context.IdeaDocuments.ToListAsync();

			if (!documents.Any())
			{
				return Array.Empty<byte>();
			}

			var baseUrl = "https://ideacollectionsystemapi20260313215839-brd4bqdwfbgeg7fj.southeastasia-01.azurewebsites.net";

			using var httpClient = new HttpClient();
			using var ms = new MemoryStream();
			using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
			{
				foreach (var doc in documents)
				{
					try
					{
						// Link thực tế của file trên Azure
						var fileUrl = baseUrl + doc.StoredPath;

						// Tải file trực tiếp từ URL
						var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

						string entryNameInsideZip = $"{doc.IdeaId}/{doc.OriginalFileName}";
						var zipEntry = archive.CreateEntry(entryNameInsideZip, CompressionLevel.Fastest);

						using var zipStream = zipEntry.Open();
						await zipStream.WriteAsync(fileBytes, 0, fileBytes.Length);
					}
					catch (Exception)
					{
						continue;
					}
				}
			}

			return ms.ToArray();
		}


		// Export CSV -> Submission ID
		public async Task<byte[]> ExportIdeasBySubmissionAsync(Guid submissionId)
		{
		
			var ideas = await _context.Ideas
				.Include(i => i.Category)
				.Where(i => i.SubmissionId == submissionId)
				.ToListAsync();

			// Tạo file CSV trong bộ nhớ (MemoryStream)
			var builder = new StringBuilder();
			builder.AppendLine("Idea ID,Title,Description,Category,Author,Created Date");

			foreach (var idea in ideas)
			{
				string authorName;
				if (idea.IsAnonymous)
				{
					authorName = "Anonymous";
				}
				else
				{
					var user = await _userManager.FindByIdAsync(idea.UserId);
					authorName = user?.Name ?? user?.Email ?? "Unknown";
				}

				// Escape dấu phẩy và nháy kép trong nội dung để không bị lỗi cột CSV
				var title = $"\"{idea.Title?.Replace("\"", "\"\"")}\"";
				var desc = $"\"{idea.Description?.Replace("\"", "\"\"")}\"";
				var category = $"\"{idea.Category?.Name?.Replace("\"", "\"\"")}\"";

				builder.AppendLine($"{idea.Id},{title},{desc},{category},{authorName},{idea.CreatedAt:yyyy-MM-dd}");
			}

			return Encoding.UTF8.GetBytes(builder.ToString());
		}

		// Export ZIP -> Submission ID
		public async Task<byte[]> ExportDocumentsBySubmissionToZipAsync(Guid submissionId)
		{
			var documents = await _context.IdeaDocuments
				.Include(d => d.Idea)
				.Where(d => d.Idea.SubmissionId == submissionId)
				.ToListAsync();

			if (!documents.Any())
			{
				return Array.Empty<byte>();
			}

	
			var baseUrl = "https://ideacollectionsystemapi20260313215839-brd4bqdwfbgeg7fj.southeastasia-01.azurewebsites.net";

			using var ms = new MemoryStream();
			int entryCount = 0;
			using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
			using (var httpClient = new HttpClient()) 
			{
				foreach (var doc in documents)
				{
					try
					{
						var relativePath = doc.StoredPath.TrimStart('/');
						var fileUrl = $"{baseUrl}/{relativePath}";

						// Tải nội dung file từ URL Azure về
						var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

						// Tên file hiển thị bên trong file ZIP (Ví dụ: ID_Idea/TenFileGoc.pdf)
						string entryNameInsideZip = $"{doc.IdeaId}/{doc.OriginalFileName}";

						var zipEntry = archive.CreateEntry(entryNameInsideZip, CompressionLevel.Fastest);
						using var zipStream = zipEntry.Open();

						await zipStream.WriteAsync(fileBytes, 0, fileBytes.Length);
						entryCount++;
					}
					catch (HttpRequestException)
					{
						Console.WriteLine($"[EXPORT ZIP] Bỏ qua file vì không tồn tại URL: {doc.StoredPath}");
						continue;
					}
				}
			}

			var resultBytes = ms.ToArray();
			if (resultBytes.Length == 0 || entryCount == 0)
			{
				return Array.Empty<byte>();
			}

			return resultBytes;
		}
	}
}