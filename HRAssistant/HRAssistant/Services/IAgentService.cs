namespace HRAssistant.Services
{
    public interface IAgentService
    {
        IAsyncEnumerable<string> GetStreamingResponse(string message);
    }
}
