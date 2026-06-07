namespace HRAssistant.Services
{
    public interface IAgentService
    {
        IAsyncEnumerable<string> GetChatResponse(string message, CancellationToken cancellationToken);
    }
}
