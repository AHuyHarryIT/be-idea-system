using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,QA Manager")]
public class ExportController : ControllerBase
{
    private readonly IQAManagerService _qaService;

    public ExportController(IQAManagerService qaService)
    {
        _qaService = qaService;
    }

    // GET api/export/csv
    [HttpGet("csv")]
    public async Task<IActionResult> ExportCsv()
    {
        var data = await _qaService.ExportIdeasToCsvAsync();
        return File(data, "text/csv", $"Ideas_{DateTime.Now:yyyyMMdd}.csv");
    }

    // GET api/export/zip
    [HttpGet("zip")]
    public async Task<IActionResult> ExportZip()
    {
        var data = await _qaService.ExportDocumentsToZipAsync();
        return File(data, "application/zip", $"Documents_{DateTime.Now:yyyyMMdd}.zip");
    }
}
