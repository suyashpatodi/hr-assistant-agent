using Microsoft.AspNetCore.Mvc;

namespace HRAssistant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private readonly IDataService _dataService;
        public DataController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpPost("ingest")]
        public async Task<IActionResult> InjestData(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".docx" && extension != ".pdf")
                return BadRequest("Only .docx and .pdf files are supported.");

            await _dataService.IngestDocumentAsync(file);

            return Ok("Document ingested successfully.");
        }
    }
}
