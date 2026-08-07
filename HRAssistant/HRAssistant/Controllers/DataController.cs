using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRAssistant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : BaseController
    {
        private readonly IDataService _dataService;
        public DataController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpPost("ingest")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> InjestData(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".docx" && extension != ".pdf")
                return BadRequest(new { message = "Only .docx and .pdf files are supported." });

            await _dataService.IngestDocumentAsync(file);

            return Ok(new { message = "Document ingested successfully." });
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeDataInfo()
        {
            var email = Email;
            var employee = await _dataService.GetEmployeeData(email!);

            if (employee == null)
                return NotFound(new { message = "Employee record not found." });

            return Ok(employee);
        }
    }
}
