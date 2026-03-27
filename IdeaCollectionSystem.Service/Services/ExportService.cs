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

		// Export ZIP (TẤT CẢ)
		public async Task<byte[]> ExportDocumentsToZipAsync()
		{
			var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

			if (!Directory.Exists(uploadPath))
			{
				return Array.Empty<byte>();
			}

			// 2. LẤY FILE VÀ KIỂM TRA: Nếu thư mục tồn tại nhưng trống trơn (empty)
			var files = Directory.GetFiles(uploadPath);
			if (files.Length == 0)
			{
				return Array.Empty<byte>(); // Trả về mảng rỗng để Controller báo lỗi 400 Bad Request
			}

			// 3. Tiến hành nén ZIP khi chắc chắn đã có file
			using var memoryStream = new MemoryStream();
			using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
			{
				foreach (var file in files)
				{
					archive.CreateEntryFromFile(file, Path.GetFileName(file));
				}
			}
			return memoryStream.ToArray();
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
			// 1. Tìm thông tin tài liệu thuộc về Submission này trong Database
			var documents = await _context.IdeaDocuments
				.Include(d => d.Idea)
				.Where(d => d.Idea.SubmissionId == submissionId)
				.ToListAsync();

			// 2. Bỏ qua những file bị lỗi/mất trên ổ cứng, chỉ lấy những file CÓ THẬT
			var validFiles = documents.Where(doc => System.IO.File.Exists(doc.StoredPath)).ToList();

			// 3. NẾU KHÔNG CÓ FILE NÀO TỒN TẠI THẬT 
			if (!validFiles.Any())
			{
				return Array.Empty<byte>();
			}

			// 4. Tiến hành nén ZIP với các file đã qua vòng kiểm duyệt
			using var ms = new MemoryStream();
			using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
			{
				foreach (var doc in validFiles)
				{
					string entryNameInsideZip = $"{doc.IdeaId}/{doc.OriginalFileName}";

					var zipEntry = archive.CreateEntry(entryNameInsideZip, CompressionLevel.Fastest);
					using var zipStream = zipEntry.Open();
					using var fileStream = new FileStream(doc.StoredPath, FileMode.Open, FileAccess.Read);
					await fileStream.CopyToAsync(zipStream);
				}
			}

			return ms.ToArray();
		}
	}
}