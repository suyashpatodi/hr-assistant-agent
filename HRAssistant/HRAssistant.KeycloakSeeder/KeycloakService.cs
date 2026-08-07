using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HRAssistant.KeycloakSeeder;

public class KeycloakService(HttpClient client)
{
    public async Task<string> GetTokenAsync(string username, string password)
    {
        var endpoint = "realms/master/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "admin-cli"),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
        ]);

        var response = await client.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var tokenData = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenData?.AccessToken ?? throw new InvalidOperationException("Failed to acquire access token.");
    }

    public async Task<bool> CreateRealm(string realm, string token)
    {
        var endpoint = "admin/realms";
        CreateRealmRequest newRealm = new(realm, true);

        using var response = await PostWithTokenAsync(endpoint, newRealm, token);

        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Conflict)
        {
            Console.WriteLine($"[INFO] Realm '{realm}' created or already exists.");
            return true;
        }

        Console.WriteLine($"[ERROR] Failed to create realm '{realm}'. Status: {response.StatusCode}");
        return false;
    }

    public async Task<List<RoleResult>> CreateRoles(List<string> roles, string realm, string token)
    {
        var results = new List<RoleResult>();
        var endpoint = $"admin/realms/{realm}/roles";

        foreach (var role in roles)
        {
            CreateRoleRequest newRole = new(role, $"{role} role");

            using var response = await PostWithTokenAsync(endpoint, newRole, token);

            if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.Conflict)
            {
                var roleDetailUrl = $"{endpoint}/{Uri.EscapeDataString(role)}";
                var existingRole = await GetWithTokenAsync<KeycloakRoleDto>(roleDetailUrl, token);

                if (existingRole is not null)
                {
                    Console.WriteLine($"[INFO] Role '{role}' resolved with ID: {existingRole.Id}");
                    results.Add(new RoleResult(existingRole.Name, existingRole.Id));
                }
            }
            else
            {
                Console.WriteLine($"[ERROR] Failed to process role '{role}'. Status: {response.StatusCode}");
            }
        }

        return results;
    }

    public async Task<bool> CreateClientApp(string clientId, string realm, string token)
    {
        var endpoint = $"admin/realms/{realm}/clients";

        var clientPayload = new
        {
            clientId = clientId,
            enabled = true,
            publicClient = true,
            standardFlowEnabled = true,        // REQUIRED for PKCE Redirect Flow
            directAccessGrantsEnabled = true, // Optional fallback for direct logins
            protocol = "openid-connect",

            // MUST match the Aspire frontend port (http://localhost:5173)
            redirectUris = new[]
            {
            "http://localhost:5173/*",
            "http://localhost:5173"
        },

            // CORS origins for browser fetch calls
            webOrigins = new[]
            {
            "http://localhost:5173"
        }
        };

        using var response = await PostWithTokenAsync(endpoint, clientPayload, token);
        return response.StatusCode == System.Net.HttpStatusCode.Created ||
               response.StatusCode == System.Net.HttpStatusCode.Conflict;
    }

    public async Task<List<UserResult>> CreateUsers(List<UserSeed> users, string realm, string token)
    {
        var results = new List<UserResult>();
        var endpoint = $"admin/realms/{realm}/users";

        foreach (var user in users)
        {
            var credentials = new List<CredentialPayload>
            {
                new(Type: "password", Value: user.Password, Temporary: false)
            };

            CreateUserRequest newUser = new(
                Username: user.Username,
                Enabled: true,
                FirstName: user.Firstname,
                LastName: user.Lastname,
                Email: user.Email,
                Credentials: credentials
            );

            using var response = await PostWithTokenAsync(endpoint, newUser, token);
            string? userId = null;

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var location = response.Headers.Location;
                userId = location?.Segments.LastOrDefault()?.TrimEnd('/');
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                // User exists -> fetch existing user ID
                var userSearchUrl = $"{endpoint}?username={Uri.EscapeDataString(user.Username)}&exact=true";
                var existingUsers = await GetWithTokenAsync<List<KeycloakUserDto>>(userSearchUrl, token);
                userId = existingUsers?.FirstOrDefault()?.Id;

                // Reset password for existing user
                if (!string.IsNullOrEmpty(userId))
                {
                    await ResetPasswordAsync(userId, user.Password, realm, token);
                }
            }

            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"[INFO] User '{user.Username}' resolved with ID: {userId}");
                results.Add(new UserResult(user.Username, userId));
            }
            else
            {
                Console.WriteLine($"[ERROR] Failed to process user '{user.Username}'. Status: {response.StatusCode}");
            }
        }

        return results;
    }

    public async Task<bool> AssignRolesToUser(string userId, List<RoleResult> rolesToAssign, string realm, string token)
    {
        var endpoint = $"admin/realms/{realm}/users/{userId}/role-mappings/realm";

        // Keycloak requires exact array of objects with "id" and "name"
        var payload = rolesToAssign.Select(r => new { id = r.Id, name = r.Name }).ToList();

        using var response = await PostWithTokenAsync(endpoint, payload, token);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[INFO] Assigned {rolesToAssign.Count} role(s) to user ID '{userId}'.");
            return true;
        }

        var errorResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[ERROR] Failed to assign roles to user ID '{userId}'. Status: {response.StatusCode}, Details: {errorResponse}");
        return false;
    }

    private async Task ResetPasswordAsync(string userId, string newPassword, string realm, string token)
    {
        var endpoint = $"admin/realms/{realm}/users/{userId}/reset-password";
        var payload = new CredentialPayload(Type: "password", Value: newPassword, Temporary: false);

        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[INFO] Successfully updated password for user ID '{userId}'.");
        }
        else
        {
            Console.WriteLine($"[WARN] Could not update password for user ID '{userId}'. Status: {response.StatusCode}");
        }
    }

    private async Task<HttpResponseMessage> PostWithTokenAsync<T>(string endpoint, T value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.SendAsync(request);
    }

    private async Task<T?> GetWithTokenAsync<T>(string endpoint, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }
}