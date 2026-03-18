using IdeaCollectionIdea.Common.Constants; 
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
	public class ExportController : ControllerBase
	{
		private readonly IExportService _exportService;

		public ExportController(IExportService exportService)
		{
			_exportService = exportService;
		}

		// GET api/export/csv
		[HttpGet("csv")]
		public async Task<IActionResult> ExportCsv()
		{
			var data = await _exportService.ExportIdeasToCsvAsync();

		
			if (data == null || data.Length == 0)
			{
				return NotFound(new { message = "There is no idea data available to export as a CSV." });
			}


			return File(data, "text/csv", $"Ideas_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
		}

		// GET api/export/zip
		[HttpGet("zip")]
		public async Task<IActionResult> ExportZip()
		{
			var data = await _exportService.ExportDocumentsToZipAsync();

			// CHỐT CHẶN: Tránh lỗi 500 nếu không có file đính kèm
			if (data == null || data.Length == 0)
			{
				return NotFound(new { message = "There are no attached documents in the system to export the ZIP code."});
				}

			return File(data, "application/zip", $"Documents_{DateTime.UtcNow:yyyyMMdd_HHmm}.zip");
		}
	}
}