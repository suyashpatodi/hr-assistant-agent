var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

#region keycloak setup
var password = builder.AddParameter("keycloak-password", secret: true);
var username = builder.AddParameter("keycloak-username", secret: true);

var keycloak = builder.AddKeycloak("keycloak",
                                    port: 8080,
                                    adminUsername: username,
                                    adminPassword: password)
                                    .WithDataVolume()
                                    .WithLifetime(ContainerLifetime.Persistent);

var keycloakSeeder = builder.AddProject<Projects.HRAssistant_KeycloakSeeder>("hrassistant-keycloakseeder")
    .WithReference(keycloak)
    .WithEnvironment("Parameters:keycloak-username", username)
    .WithEnvironment("Parameters:keycloak-password", password)
    .WaitFor(keycloak);
#endregion

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

#region reference
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
    .WithEnvironment("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"))
    .WithHttpEndpoint(port: 5173)
    .WithReference(api)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WaitFor(api);
#endregion

builder.Build().Run();