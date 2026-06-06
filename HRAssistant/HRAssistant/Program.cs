using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler().Configure(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
    });
});

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddOllamaApiClient("ollama-llama3-2").AddChatClient();
builder.AddOllamaApiClient("ollama-all-minilm").AddEmbeddingGenerator();

builder.Services.AddInMemoryVectorStoreRecordCollection<string, DocumentChunk>("documents");

builder.Services.AddTransient<ChatOptions>(_ => new ChatOptions()
{
    Temperature = 0.9f,
    ToolMode = ChatToolMode.Auto
});

//builder.Services.AddSemanticKernelDependencies(builder.Configuration);

builder.Services.AddScoped<IAgentService, AgentService>();

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
