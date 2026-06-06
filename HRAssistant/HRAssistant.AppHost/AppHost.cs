var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var ollama = builder.AddOllama("ollama", 11434)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithOpenWebUI();

var llama = ollama.AddModel("ollama-llama3-2", "llama3.2");
var embedding = ollama.AddModel("ollama-all-minilm", "all-minilm");

builder.AddProject<Projects.HRAssistant>("hrassistant")
    .WithReference(llama)
    .WithReference(embedding)
    .WaitFor(llama)
    .WaitFor(embedding);

builder.Build().Run();
