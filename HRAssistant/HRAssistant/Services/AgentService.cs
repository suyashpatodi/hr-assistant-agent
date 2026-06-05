using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace HRAssistant.Services
{
    public class AgentService : IAgentService
    {
        private readonly Kernel _kernel;
        private readonly PromptExecutionSettings _settings;
        private readonly IChatCompletionService _chatService;

        public AgentService(IChatCompletionService chatService, Kernel kernel, PromptExecutionSettings settings)
        {
            _kernel = kernel;
            _settings = settings;
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }
        public async IAsyncEnumerable<string> GetStreamingResponse(string message)
        {
            var history = new ChatHistory();

            history.AddUserMessage(message);

            var response = _chatService.GetStreamingChatMessageContentsAsync(history, _settings, _kernel);
            string fullResponse = string.Empty;

            await foreach (var chunk in response)
            {
                if (chunk.Content != null)
                {
                    fullResponse += chunk.Content;
                    yield return chunk.Content;
                }
            }

            history.AddAssistantMessage(fullResponse);
        }
    }
}
