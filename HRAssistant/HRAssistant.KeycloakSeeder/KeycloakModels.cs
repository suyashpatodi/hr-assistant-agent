using System.Text.Json.Serialization;

namespace HRAssistant.KeycloakSeeder;

// JSON Seed Models (Matching data.json)
public record KeycloakSeedData(
    string Realm,
    string ClientId,
    List<string> Roles,
    List<UserSeed> Users
);

public record UserSeed(
    string Username,
    string Email,
    string Firstname,
    string Lastname,
    string Password,
    string Role
);

// Response / Result Models
public record RoleResult(string Name, string Id);
public record UserResult(string Username, string Id);

// Keycloak REST API Request Payloads
public record CreateRealmRequest(string Realm, bool Enabled);

public record CreateRoleRequest(string Name, string Description);

public record CreateUserRequest(
    string Username,
    bool Enabled,
    string FirstName,
    string LastName,
    string Email,
    List<CredentialPayload> Credentials
);

public record CredentialPayload(
    string Type,
    string Value,
    bool Temporary
);

// Internal Keycloak Response DTOs
public record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken
);

internal record KeycloakRoleDto(string Id, string Name);
internal record KeycloakUserDto(string Id, string Username);