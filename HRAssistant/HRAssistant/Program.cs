using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler().Configure(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(5);
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
    });
});

builder.AddNpgsqlDbContext<EmployeeDbContext>("company");
builder.AddRedisDistributedCache(connectionName: "cache");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<PolicyEnquiry>();
builder.Services.AddScoped<SqlEnquiry>();
builder.Services.AddScoped<ExecuteAction>();

// Register the universal IChatClient pipeline targeting Gemini 2.5 Pro
builder.Services.AddScoped<Kernel>(sp =>
{
    var apiKey = builder.Configuration["Gemini:ApiKey"]!;

    var kernelBuilder = Kernel.CreateBuilder();

    // Use the native Google AI Studio Connector
    kernelBuilder.AddGoogleAIGeminiChatCompletion(
        modelId: "gemini-2.5-flash",
        apiKey: apiKey
    );

    var kernel = kernelBuilder.Build();

    kernel.Plugins.AddFromObject(sp.GetRequiredService<SqlEnquiry>(), "SqlEnquiry");
    kernel.Plugins.AddFromObject(sp.GetRequiredService<PolicyEnquiry>(), "PolicyEnquiry");
    kernel.Plugins.AddFromObject(sp.GetRequiredService<ExecuteAction>(), "ExecuteAction");

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