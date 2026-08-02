var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache")
                    .WithRedisInsight()
                    .WithDataVolume()
                    .WithLifetime(ContainerLifetime.Persistent); ;

var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin()
                      .WithDataVolume()
                      .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("company");

var ollama = builder.AddOllama("ollama", 11434)
                    .WithDataVolume()
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithOpenWebUI();
var embedding = ollama.AddModel("all-minilm");

var geminiKey = builder.AddParameter("gemini-apikey", secret: true);

var api = builder.AddProject<Projects.HRAssistant>("hrassistant")
    .WithEnvironment("Gemini__ApiKey", geminiKey)
    .WithReference(redis)
    .WithReference(embedding)
    .WithReference(database)
    .WaitFor(embedding)
    .WaitFor(redis)
    .WaitFor(database);

var frontend = builder.AddViteApp("hrassistantfrontend", "../HRAssistant.Frontend")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
