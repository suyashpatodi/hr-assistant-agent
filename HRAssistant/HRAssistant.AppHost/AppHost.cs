var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin()
                      .WithDataVolume()
                      .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("company");

var groqKey = builder.AddParameter("groq-apikey", secret: true);

var api = builder.AddProject<Projects.HRAssistant>("hrassistant")
    .WithEnvironment("Groq__ApiKey", groqKey)
    .WithReference(database)
    .WaitFor(database);

var frontend = builder.AddViteApp("hrassistantfrontend", "../HRAssistant.Frontend")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
