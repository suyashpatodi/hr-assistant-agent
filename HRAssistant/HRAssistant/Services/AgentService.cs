using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.CompilerServices;
using System.Text.Json;

public class AgentService : IAgentService
{
    private readonly Kernel _kernel;
    private readonly IDistributedCache _redis;

    public AgentService(Kernel kernel, IDistributedCache redis)
    {
        _kernel = kernel;
        _redis = redis;
    }

    public async IAsyncEnumerable<string> GetChatResponse(
        string message,
        string key,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();

        var cached = await _redis.GetStringAsync(key, cancellationToken);
        if (cached != null)
        {
            List<StoredMessage>? stored = null;

            try
            {
                stored = JsonSerializer.Deserialize<List<StoredMessage>>(cached);
            }
            catch (JsonException)
            {
                await _redis.RemoveAsync(key, cancellationToken);
            }

            if (stored is { Count: > 0 })
            {
                history.AddSystemMessage(stored[0].Content);
                foreach (var msg in stored.Skip(1))
                {
                    if (msg.Role == "user")
                        history.AddUserMessage(msg.Content);
                    else if (msg.Role == "assistant")
                        history.AddAssistantMessage(msg.Content);
                }
            }
            else
            {
                AddSystemPrompt(history, key);
            }
        }
        else
        {
            AddSystemPrompt(history, key);
        }

        history.AddUserMessage(message);

        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var results = await chatCompletion.GetChatMessageContentsAsync(
            history, executionSettings, _kernel, cancellationToken);

        string fullAssistantResponse = string.Empty;

        foreach (var result in results)
            if (!string.IsNullOrEmpty(result.Content))
                fullAssistantResponse += result.Content;

        history.AddAssistantMessage(fullAssistantResponse);

        var toStore = new List<StoredMessage>
        {
            new("system", history.First(m => m.Role == AuthorRole.System).Content ?? "")
        };

        foreach (var msg in history.Where(m => m.Role == AuthorRole.User || m.Role == AuthorRole.Assistant))
        {
            var text = string.Join("", msg.Items.OfType<Microsoft.SemanticKernel.TextContent>().Select(t => t.Text));
            if (!string.IsNullOrWhiteSpace(text))
                toStore.Add(new(msg.Role == AuthorRole.User ? "user" : "assistant", text));
        }

        await _redis.SetStringAsync(key, JsonSerializer.Serialize(toStore), cancellationToken);

        foreach (var word in fullAssistantResponse.Split(' '))
        {
            yield return word + " ";
            await Task.Delay(10, cancellationToken); // optional: feels like streaming
        }
    }

    private static void AddSystemPrompt(ChatHistory history, string key)
    {
        history.AddSystemMessage($"""
            You are an HR Assistant. Always use tools — never answer from memory. And you only reply for employee {key}
            - SqlPlugin: Leave balances and history
            - PolicyPlugin: Search companies policy document to fetch company related information including travel plans, compensation, hierarchy, company goals, ongoing projects and many more.
            - ExecutePlugin: Submit leave applications and send emails. Always confirm with user before executing.

            Response rules:
            - Return only what was asked. No extra commentary or suggestions.
            - If a tool explicitly returns that a user or record was not found, stop calling functions and politely inform the user that the record does not exist.
            """);
    }

    private record StoredMessage(string Role, string Content);
}