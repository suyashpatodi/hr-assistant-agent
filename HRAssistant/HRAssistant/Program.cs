using Azure.AI.Inference;
using HRAssistant.Plugins;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler().Configure(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(15);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(30);
        options.Retry.MaxRetryAttempts = 1; // Never retry — slow LLM + retry = guaranteed timeout
    });
});

// GitHub Models via Aspire — registers ChatCompletionsClient
builder.AddAzureChatCompletionsClient("ai-models");

// Bridge ChatCompletionsClient → IChatCompletionService for Semantic Kernel
builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    var completionsClient = sp.GetRequiredService<ChatCompletionsClient>();
    IChatClient chatClient = completionsClient.AsIChatClient("gpt-4o-mini");
    return chatClient.AsChatCompletionService();
});

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Aspire wires these up from WithReference() in AppHost
builder.AddOllamaApiClient("ollama-all-minilm").AddEmbeddingGenerator();

builder.Services.AddSingleton<PolicyEnquiry>();
builder.Services.AddSingleton<SqlEnquiry>();
builder.Services.AddSingleton<ExecuteAction>();

builder.Services.AddKernel().Plugins
    .AddFromType<SqlEnquiry>("SqlPlugin")
    .AddFromType<PolicyEnquiry>("PolicyPlugin")
    .AddFromType<ExecuteAction>("ExecutePlugin");

builder.Services.AddInMemoryVectorStoreRecordCollection<string, DocumentChunk>("documents");

builder.Services.AddTransient<IAgentService, AgentService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
