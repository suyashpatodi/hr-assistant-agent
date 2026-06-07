using Aspire.Hosting.GitHub;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var githubModels = builder.AddGitHubModel("ai-models", GitHubModel.OpenAI.OpenAIGpt41Mini);

var ollama = builder.AddOllama("ollama", 11434)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithOpenWebUI();

var embedding = ollama.AddModel("ollama-all-minilm", "all-minilm");

builder.AddProject<Projects.HRAssistant>("hrassistant")
    .WithReference(githubModels)
    .WithReference(embedding)
    .WaitFor(githubModels)
    .WaitFor(embedding);

builder.Build().Run();
