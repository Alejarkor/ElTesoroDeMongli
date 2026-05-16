# API client layer

This folder contains the first centralized API client layer for the PHP API.

## Current purpose

Provide a single place for:

```text
login
register
get_users
update_users
```

without scattering endpoint URLs and `UnityWebRequest` calls across gameplay/UI scripts.

## Dependencies

This layer depends on `EnvironmentConfig` for the API base URL.

## Example usage

```csharp
EnvironmentConfig config = EnvironmentConfigProvider.Current;
PhpApiClient apiClient = new PhpApiClient(config);
SessionService sessionService = new SessionService();
AuthService authService = new AuthService(apiClient, sessionService);

LoginResponse response = await authService.LoginAsync("test@example.com", "Test1234!");

if (response.IsSuccess)
{
    Debug.Log($"Logged in as user {sessionService.CurrentSession.UserId}");
}
else
{
    Debug.LogWarning($"Login failed with error {response.error_code}");
}
```

## Next step

Wire the existing login/register UI to `AuthService` after locating the current UI scripts in Unity.
