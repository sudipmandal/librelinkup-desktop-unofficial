# LibreLinkUp API Implementation

## Overview

The application integrates with the LibreLinkUp API to authenticate users and fetch CGM (Continuous Glucose Monitor) data. All API calls are made via `HttpClient` from the `LinkupService` class.

## API Functions

### 1. GetAuthTokenAsync()

Located in `Services/LinkupService.cs`, authenticates with LibreLinkUp and returns an authentication token.

**Parameters:**
```csharp
string country   // Country code (global, us, ca, eu, etc.)
string username   // Email address
string password   // Password
```

**Returns:**
```csharp
(LoginResult? Result, string? Error)
// LoginResult: { Token, AccountId, AccountCountry }
```

### 2. GetCgmDataAsync()

Located in `Services/LinkupService.cs`, fetches CGM data including current glucose reading.

**Parameters:**
```csharp
string token      // Authentication token from login
string country    // Country code
string accountId  // User account ID
```

**Returns:**
```csharp
(CgmConnectionData? Data, string? Error)
// CgmConnectionData: { GlucoseMeasurement, FirstName, LastName }
```

## Login Flow

1. User enters credentials (country, email, password)
2. `LinkupService.GetAuthTokenAsync()` posts to LibreLinkUp API
3. Response may redirect to country-specific endpoint
4. Token, account ID, and country are stored in settings
5. CGM data is automatically fetched
6. UI switches to compact CGM display view

## Base URL Construction

```csharp
private static string GetBaseUrl(string countryCode)
{
    const string template = "https://api-COUNTRY_CODE.libreview.io/llu";
    if (countryCode == "global")
        return template.Replace("-COUNTRY_CODE", "");
    return template.Replace("COUNTRY_CODE", countryCode.ToLower());
}
```

## SHA256 Hash Utility

Used for the `Account-Id` header:
```csharp
private static string Hash256(string input)
{
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = SHA256.HashData(bytes);
    return Convert.ToHexStringLower(hash);
}
```

## Required Headers

All LibreLinkUp API requests include:
- `product: llu.android`
- `version: 4.16.0`
- `Pragma: no-cache`
- `Cache-Control: no-cache`
- `Authorization: Bearer {token}` (authenticated endpoints)
- `Account-Id: {sha256(accountId)}` (authenticated endpoints)

## Supported Countries

| Code | Region |
|------|--------|
| global | Global (default) |
| de | Germany |
| eu | European Union |
| eu2 | European Union 2 |
| us | United States |
| ap | Asia/Pacific |
| ca | Canada |
| jp | Japan |
| ae | United Arab Emirates |
| fr | France |
| au | Australia |

## Data Persistence

Settings are stored as JSON in the user's AppData directory:

| Field | Purpose |
|-------|---------|
| Country | Selected country code |
| Email | User email |
| Password | User password |
| AlwaysOnTop | Window preference |
| Token | Authentication token |
| AccountId | User account ID |
| AccountCountry | Account country |

**File location:**
- Windows: `%APPDATA%\LibreLinkupDesktop-Unofficial\settings.json`
- Linux: `~/.config/LibreLinkupDesktop-Unofficial/settings.json`

## Error Handling

The service provides clear error messages for:
- Authentication errors (invalid credentials)
- No connections found
- Network timeouts (30 second limit)
- Server errors

Failed logins trigger auto-retry every 10 seconds.
