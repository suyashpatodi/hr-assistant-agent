using HRAssistant.Services;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace HRAssistant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;
        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpGet("stream")]
        [Produces("text/stream")]
        public async Task StreamContent([FromQuery] string message, [EnumeratorCancellation] CancellationToken ct)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            var streamingResult = _agentService.GetStreamingResponse(message);

            await foreach (var chunk in streamingResult.WithCancellation(ct))
            {
                await Response.WriteAsync($"data: {chunk}\n\n");
                await Response.Body.FlushAsync();
            }
        }
    }
}
