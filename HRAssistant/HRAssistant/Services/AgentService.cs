using System.Runtime.CompilerServices;

public class AgentService : IAgentService
{
    private readonly Kernel _kernel;

    public AgentService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async IAsyncEnumerable<string> GetChatResponse(
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddSystemMessage("""
            You are an HR Assistant. You MUST use tools to answer questions.
            - Employee data: use SqlPlugin
            - Policy questions: use PolicyPlugin
            Always call the appropriate tool and summarize the result.
            """);
        history.AddUserMessage(message);

        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var response = await chatCompletion.GetChatMessageContentAsync(
            history,
            executionSettings: executionSettings,
            kernel: _kernel,
            cancellationToken: cancellationToken
        );

        foreach (var word in response.Content?.Split(' ') ?? [])
        {
            yield return word + " ";
            await Task.Delay(20, cancellationToken);
        }
    }
}