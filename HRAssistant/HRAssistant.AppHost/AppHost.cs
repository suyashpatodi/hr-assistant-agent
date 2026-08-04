var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var password = builder.AddParameter("keycloak-password", secret: true);
var username = builder.AddParameter("keycloak-username", secret: true);

var keycloak = builder.AddKeycloak("keycloak", adminUsername: username, adminPassword: password);

#region database setup
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("company");
#endregion

#region AI setup
var ollama = builder.AddOllama("ollama", 11434)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithOpenWebUI();

var embedding = ollama.AddModel("all-minilm");

var geminiKey = builder.AddParameter("gemini-apikey", secret: true);
#endregion

var keycloakSeeder = builder.AddProject<Projects.HRAssistant_KeycloakSeeder>("hrassistant-keycloakseeder")
    .WithReference(keycloak)
    .WithEnvironment("Parameters:keycloak-username", username)
    .WithEnvironment("Parameters:keycloak-password", password)
    .WaitFor(keycloak);

var api = builder.AddProject<Projects.HRAssistant>("hrassistant")
    .WithEnvironment("Gemini__ApiKey", geminiKey)
    .WithReference(redis)
    .WithReference(keycloak)
    .WithReference(embedding)
    .WithReference(database)
    .WaitFor(embedding)
    .WaitFor(keycloak)
    .WaitFor(keycloakSeeder)
    .WaitFor(redis)
    .WaitFor(database);

var frontend = builder.AddViteApp("hrassistantfrontend", "../HRAssistant.Frontend")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();