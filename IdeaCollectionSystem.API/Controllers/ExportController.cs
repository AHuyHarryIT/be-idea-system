using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[ApiController]
	[Route("api/export")]
	[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
	public class ExportController : ControllerBase
	{
		private readonly IExportService _exportService;

		public ExportController(IExportService exportService)
		{
			_exportService = exportService;
		}

		// 1. Tải CSV 
		[HttpGet("csv")]
		public async Task<IActionResult> ExportCsv()
		{
			var data = await _exportService.ExportIdeasToCsvAsync();
			if (data == null || data.Length == 0)
				return NotFound(new { message = "There is no idea data available to export as a CSV." });

			return File(data, "text/csv", $"Ideas_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
		}

		// 2. Tải ZIP 
		[HttpGet("zip")]
		public async Task<IActionResult> ExportZip()
		{
			var data = await _exportService.ExportDocumentsToZipAsync();
			if (data == null || data.Length == 0)
				return NotFound(new { message = "There are no attached documents in the system to export the ZIP file." });

			return File(data, "application/zip", $"Documents_{DateTime.UtcNow:yyyyMMdd_HHmm}.zip");
		}

		// 3. Tải CSV (LỌC THEO SUBMISSION ID)
		[HttpGet("csv/{submissionId}")]
		public async Task<IActionResult> ExportIdeasBySubmission(Guid submissionId)
		{
			var fileBytes = await _exportService.ExportIdeasBySubmissionAsync(submissionId);

			// Validate: Không có Idea thì không cho tải
			if (fileBytes == null || fileBytes.Length == 0)
				return NotFound(new { message = "No ideas found for this submission to export." });

			return File(fileBytes, "text/csv", $"Ideas_Submission_{submissionId}.csv");
		}

		// 4. Tải ZIP (LỌC THEO SUBMISSION ID)
		[HttpGet("zip/{submissionId}")]
		public async Task<IActionResult> DownloadDocumentsAsZipBySubmission(Guid submissionId)
		{
			
			var fileBytes = await _exportService.ExportDocumentsBySubmissionToZipAsync(submissionId);

			if (fileBytes == null || fileBytes.Length == 0)
			{
				return BadRequest(new { message = "There are no attached documents in this submission to download!" });
			}

			return File(fileBytes, "application/zip", $"Documents_Submission_{submissionId}.zip");
		}
	}
}