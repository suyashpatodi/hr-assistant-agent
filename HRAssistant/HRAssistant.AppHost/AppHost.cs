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

var geminiKey = builder.AddParameter("gemini-apikey", secret: true);

var api = builder.AddProject<Projects.HRAssistant>("hrassistant")
    .WithEnvironment("Gemini__ApiKey", geminiKey)
    .WithReference(redis)
    .WithReference(database)
    .WaitFor(redis)
    .WaitFor(database);

var frontend = builder.AddViteApp("hrassistantfrontend", "../HRAssistant.Frontend")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
