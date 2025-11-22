# LibreLinkUp API Implementation

## Overview
The application includes complete LibreLinkUp API integration for authenticating and fetching CGM (Continuous Glucose Monitor) data. All API calls are made directly from the frontend using Tauri's HTTP plugin to bypass CORS restrictions.

## Frontend API Functions

### 1. getAuthToken()
Located in `src/lib/linkup.ts`, authenticates with LibreLinkUp and returns authentication token.

**Parameters:**
```typescript
{
  country: string;    // Country code (global, us, ca, eu, etc.)
  username: string;   // Email address
  password: string;   // Password
}
```

**Response:**
```typescript
{
  token: string;           // JWT authentication token
  accountId: string;       // User account ID
  accountCountry: string;  // Account country code
}
// Or { error: string } on failure
```

### 2. getCGMData()
Located in `src/lib/linkup.ts`, fetches CGM data including current glucose reading and sensor info.

**Parameters:**
```typescript
{
  token: string;      // Authentication token from login
  country: string;    // Country code
  accountId: string;  // User account ID
}
```

**Response:**
Returns the complete connection data including:
- `glucoseMeasurement`: Current glucose value, trend arrow, timestamp
- `firstName`, `lastName`: Patient name
- `sensor`: Device ID and serial number
- Full graph data history

### 3. getConnection()
Located in `src/lib/linkup.ts`, lists all LibreLinkUp connections for the authenticated user.

**Parameters:**
```typescript
{
  token: string;      // Authentication token
  country: string;    // Country code
  accountId: string;  // User account ID
}
```

**Response:**
Returns the first connection object with patient details.

## Frontend Integration

### Login Flow
1. User enters credentials (country, email, password)
2. Frontend calls `/api/linkup/login`
3. Backend authenticates with LibreLinkUp API
4. Token and account ID are stored in Tauri store
5. CGM data is automatically fetched
6. UI switches to CGM display view

### CGM Data Display
After successful login, the UI shows:
- **Patient Name**: From connection data
- **Current Glucose Value**: Large display with unit (mg/dL or mmol/L)
- **Trend Arrow**: Visual indicator (↑↑, ↑, →, ↓, ↓↓)
- **Timestamp**: Last reading time
- **Sensor Info**: Device ID and serial number
- **Refresh Button**: Manually fetch latest data
- **Logout Button**: Return to login form

### Data Persistence
The following values are saved in Tauri store (`settings.json`):
- `country`: Selected country code
- `email`: User email
- `password`: Plain text (for convenience)
- `alwaysOnTop`: Window preference
- `token`: Authentication token (after login)
- `accountId`: User account ID (after login)
- `accountCountry`: Account country (after login)

## API Implementation Details

### Base URL Construction
Located in `src/lib/linkup.ts`:
```typescript
function getBaseUrl(countryCode: string): string {
  const baseUrl = 'https://api-COUNTRY_CODE.libreview.io/llu';
    
    if (countryCode.ToLower() == "global")
    {
        return baseUrl.Replace("-COUNTRY_CODE", "");
    }
    
    return baseUrl.Replace("COUNTRY_CODE", countryCode.ToLower());
}
```

### SHA256 Hash Utility
Used for Account-Id header authentication:
```csharp
static string Hash256(string input)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToHexString(hash).ToLower();
}
```

### Required Headers
All LibreLinkUp API requests include:
- `product: llu.android`
- `version: 4.16.0`
- `Pragma: no-cache`
- `Cache-Control: no-cache`
- `Authorization: Bearer {token}` (for authenticated endpoints)
- `Account-Id: {hash256(accountId)}` (for authenticated endpoints)

## Supported Countries
- Global (default)
- Germany (de)
- European Union (eu)
- European Union 2 (eu2)
- United States (us)
- Asia/Pacific (ap)
- Canada (ca)
- Japan (jp)
- United Arab Emirates (ae)
- France (fr)
- Australia (au)

## Error Handling

The backend provides clear error messages for:
- **LOGIN_FAILED**: Authentication errors
- **CONNECTIONS_FAILED**: Failed to fetch connections
- **NO_CONNECTIONS**: No LibreLinkUp connections configured
- **GRAPH_FAILED**: Failed to fetch glucose data
- **CGM_FAILED**: General CGM data fetch errors

## Testing

To test the implementation:

1. **Start the C# backend:**
   ```bash
   cd src-csharp
   dotnet run
   ```

2. **Start the Tauri dev environment:**
   ```bash
   npm run tauri dev
   ```

3. **Login Flow:**
   - Enter your LibreLinkUp credentials
   - Select appropriate country
   - Click Login
   - View your CGM data

4. **Manual API Testing:**
   ```bash
   # Test login
   curl -X POST http://localhost:5000/api/linkup/login \
     -H "Content-Type: application/json" \
     -d '{"country":"global","username":"your@email.com","password":"yourpassword"}'

   # Test CGM data fetch
   curl -X POST http://localhost:5000/api/linkup/cgm \
     -H "Content-Type: application/json" \
     -d '{"token":"YOUR_TOKEN","country":"global","accountId":"YOUR_ACCOUNT_ID"}'
   ```

## Next Steps

Potential improvements:
1. **Automatic Refresh**: Set up a timer to fetch new CGM data every 1-5 minutes
2. **Graph Visualization**: Display historical glucose readings
3. **Alerts**: Notify when glucose goes above/below thresholds
4. **Multiple Connections**: Support viewing multiple patients
5. **Token Refresh**: Handle token expiration and auto-refresh
6. **Offline Mode**: Cache data for viewing when offline
7. **Export Data**: Save readings to CSV/JSON files

## Security Notes

- Password is stored in plain text in `settings.json` for convenience
- For production use, consider encrypting sensitive data
- Token should be treated as sensitive and not logged
- Consider implementing token refresh mechanism
