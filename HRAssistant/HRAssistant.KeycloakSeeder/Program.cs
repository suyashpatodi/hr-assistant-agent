using HRAssistant.KeycloakSeeder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

// Register Service Discovery
builder.Services.AddServiceDiscovery();

// Register typed HttpClient for KeycloakService
builder.Services.AddHttpClient<KeycloakService>(client =>
{
    client.BaseAddress = new Uri("https+http://keycloak");
})
.AddServiceDiscovery();

using var host = builder.Build();

var configuration = host.Services.GetRequiredService<IConfiguration>();
var keycloakService = host.Services.GetRequiredService<KeycloakService>();

string? adminUsername = configuration["Parameters:keycloak-username"];
string? adminPassword = configuration["Parameters:keycloak-password"];

if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
{
    Console.WriteLine("[ERROR] Keycloak credentials not found in configuration.");
    return;
}

try
{
    Console.WriteLine("[INFO] Requesting Keycloak Admin Token...");
    string token = await keycloakService.GetTokenAsync(adminUsername, adminPassword);

    var jsonPath = Path.Combine(AppContext.BaseDirectory, "data.json");
    if (!File.Exists(jsonPath))
    {
        Console.WriteLine($"[ERROR] Seed data file not found at: {jsonPath}");
        return;
    }

    var data = await File.ReadAllTextAsync(jsonPath);
    var options = JsonSerializer.Deserialize<KeycloakSeedData>(
        data,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    )!;

    // Create Realm
    var realmCreated = await keycloakService.CreateRealm(options.Realm, token);
    if (!realmCreated) return;

    // Create Roles
    List<RoleResult> createdRoles = await keycloakService.CreateRoles(options.Roles, options.Realm, token);

    // Create Users (with credentials or reset existing password)
    List<UserResult> createdUsers = await keycloakService.CreateUsers(options.Users, options.Realm, token);

    // Assign Single Role per User (based on data.json "Role" property)
    Console.WriteLine("[INFO] Assigning roles to users...");
    foreach (var userSeed in options.Users)
    {
        var matchedUser = createdUsers.FirstOrDefault(u => u.Username.Equals(userSeed.Username, StringComparison.OrdinalIgnoreCase));
        if (matchedUser is null) continue;

        var roleToAssign = createdRoles.Where(r => r.Name.Equals(userSeed.Role, StringComparison.OrdinalIgnoreCase)).ToList();

        if (roleToAssign.Any())
        {
            await keycloakService.AssignRolesToUser(matchedUser.Id, roleToAssign, options.Realm, token);
        }
        else
        {
            Console.WriteLine($"[WARN] Role '{userSeed.Role}' not found for user '{userSeed.Username}'.");
        }
    }

    Console.WriteLine("[SUCCESS] Keycloak Seeding completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Seeding process failed: {ex.Message}");
}