using Microsoft.Extensions.Http.Resilience;

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

// OpenAI via Aspire — registers OpenAIClient
builder.AddOllamaApiClient("gpt-model")
       .AddChatClient()
       .UseFunctionInvocation();
builder.AddNpgsqlDbContext<EmployeeDbContext>("company");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<PolicyEnquiry>();
builder.Services.AddScoped<SqlEnquiry>();
builder.Services.AddScoped<ExecuteAction>();

builder.Services.AddScoped<Kernel>(sp =>
{
    var apiKey = builder.Configuration["Groq:ApiKey"]!;

    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: "llama-3.1-8b-instant",
        apiKey: apiKey,
        endpoint: new Uri("https://api.groq.com/openai/v1")
    );

    var kernel = kernelBuilder.Build();

    kernel.Plugins.AddFromObject(sp.GetRequiredService<SqlEnquiry>(), "SqlPlugin");
    kernel.Plugins.AddFromObject(sp.GetRequiredService<PolicyEnquiry>(), "PolicyPlugin");
    kernel.Plugins.AddFromObject(sp.GetRequiredService<ExecuteAction>(), "ExecutePlugin");

    return kernel;
});

builder.Services.AddInMemoryVectorStoreRecordCollection<string, DocumentChunk>("documents");
builder.Services.AddScoped<IAgentService, AgentService>();

var app = builder.Build();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseMigration();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();