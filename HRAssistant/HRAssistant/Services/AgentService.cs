using Microsoft.Extensions.AI;

namespace HRAssistant.Services
{
    public class AgentService : IAgentService
    {
        private readonly IChatClient _chatClient;
        private readonly ChatOptions _settings;
        public AgentService(IChatClient chatClient, ChatOptions settings)
        {
            _chatClient = chatClient;
            _settings = settings;
        }
        public async IAsyncEnumerable<string> GetStreamingResponse(string message)
        {
            var chatHistory = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are an HR manager"),
                new ChatMessage(ChatRole.User, message)
            };

            var response = _chatClient.GetStreamingResponseAsync(chatHistory, _settings);
            string fullResponse = string.Empty;

            await foreach (var chunk in response)
            {
                if (chunk.Text != null)
                {
                    fullResponse += chunk.Text;
                    yield return chunk.Text;
                }
            }

            chatHistory.Add(new ChatMessage(ChatRole.Assistant, fullResponse));
        }
    }
}
