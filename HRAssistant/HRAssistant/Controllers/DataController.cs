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

        [HttpGet("ingest")]
        public async Task<IActionResult> InjestData([FromQuery] string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return BadRequest("File not found.");

            await _dataService.IngestDocumentAsync(filePath);
            return Ok("Document ingested successfully.");
        }
    }
}
