using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace HRAssistant.Helpers
{
    public static class Extension
    {
        public static IServiceCollection AddSemanticKernelDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var kernelBuilder = services.AddKernel();

            // Add Plugins

            var apiKey = configuration.GetValue<string>(Constants.GithubApiKey) ?? string.Empty;
            var model = configuration.GetValue<string>(Constants.GithubModel) ?? string.Empty;
            var endpoint = configuration.GetValue<string>(Constants.GithubEndpoint) ?? string.Empty;

            kernelBuilder.AddOpenAIChatCompletion(modelId: model, apiKey: apiKey, httpClient: new HttpClient { BaseAddress = new Uri(endpoint) });

            // Add Scope for all agents

            services.AddTransient<PromptExecutionSettings>(_ => new OpenAIPromptExecutionSettings()
            {
                Temperature = 0.9f,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            });

            return services;
        }
    }
}
