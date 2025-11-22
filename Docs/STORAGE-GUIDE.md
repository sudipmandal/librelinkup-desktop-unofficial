# Storage Guide

## Overview

This application uses **Tauri Store Plugin** for cross-platform data persistence.

The Store Plugin is used for storing:
- User preferences (country, email, alwaysOnTop setting)
- Authentication credentials (password stored in plain text for convenience)
- Session data (token, accountId, accountCountry after login)

Storage works seamlessly across **Windows, macOS, and Linux**.

---

## Store Plugin Details

| Feature | Store Plugin |
|---------|-------------|
| **Use Case** | User preferences, credentials, session data |
| **Storage Location** | App data directory (JSON file) |
| **Encryption** | ❌ Plain text JSON |
| **Visibility** | Readable in file system |
| **Cross-Platform** | ✅ Yes |
| **Performance** | Fast (in-memory + disk) |
| **Data Types** | Any JSON-serializable |

### Storage Locations

- **Windows**: `%APPDATA%\com.sudipmandal.lldunofficial\settings.json`
- **macOS**: `~/Library/Application Support/com.sudipmandal.lldunofficial/settings.json`
- **Linux**: `~/.config/com.sudipmandal.lldunofficial/settings.json`

### Security Note

Passwords are stored in plain text for convenience. For production use, consider implementing:
- OS keyring integration for secure password storage
- Token-based authentication without storing passwords
- Encrypted storage using crypto libraries

---

## Quick Start

### Import the Store Plugin

```typescript
import { Store } from '@tauri-apps/plugin-store';

// In your Vue component (mounted hook)
const store = await Store.load('settings.json');
```

### Current Implementation

The app uses the Store plugin directly in `App.vue` with the following pattern:

```typescript
// Store instance with markRaw to prevent Vue reactivity issues
this.store = markRaw(await Store.load('settings.json'));

// Save data
await this.store.set('key', value);
await this.store.save();

// Load data
const value = await this.store.get('key');
```

---

## Non-Sensitive Data Storage (Store Plugin)

### Basic Usage

```typescript
// Save preferences
await savePreference('theme', 'dark');
await savePreference('language', 'en');
await savePreference('fontSize', 14);

// Get preferences
const theme = await getPreference('theme', 'light'); // 'dark'
const lang = await getPreference('language', 'en'); // 'en'

// Delete a preference
await deletePreference('fontSize');
```

### Storing Objects

```typescript
// Store complex objects
await savePreference('windowPosition', { x: 100, y: 200, width: 800, height: 600 });
await savePreference('userSettings', {
  notifications: true,
  autoSave: false,
  theme: 'dark'
});

// Retrieve objects
const position = await getPreference<{ x: number; y: number }>('windowPosition');
const settings = await getPreference<UserSettings>('userSettings');
```

### Vue Component Example

```vue
<template>
  <div>
    <select v-model="theme" @change="saveTheme">
      <option value="light">Light</option>
      <option value="dark">Dark</option>
    </select>
  </div>
</template>

<script lang="ts">
import { savePreference, getPreference } from '@/utils/storage';

export default {
  data() {
    return {
      theme: 'light'
    };
  },
  
  async mounted() {
    // Load saved theme
    this.theme = await getPreference('theme', 'light');
  },
  
  methods: {
    async saveTheme() {
      await savePreference('theme', this.theme);
      console.log('Theme saved:', this.theme);
    }
  }
};
</script>
```

---

## Secure Data Storage (OS Keyring)

### Storing API Tokens

```typescript
// Store a token
await storeApiToken('eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...');

// Retrieve the token
const token = await getApiToken();
if (token) {
  console.log('Token found');
} else {
  console.log('No token stored');
}

// Delete the token
await deleteApiToken();
```

### Storing User Credentials

```typescript
// Store username and password
await storeCredentials('user@example.com', 'SecurePassword123!');

// Retrieve credentials
const creds = await getCredentials();
if (creds) {
  console.log('Username:', creds.username);
  console.log('Password:', creds.password);
}

// Delete credentials
await deleteCredentials();
```

### Advanced: Custom Secure Storage

```typescript
// Store custom secure values
await storeSecure('tauri-app', 'oauth-token', 'abc123xyz');
await storeSecure('tauri-app', 'refresh-token', 'refresh_xyz');

// Retrieve secure values
try {
  const oauthToken = await getSecure('tauri-app', 'oauth-token');
  const refreshToken = await getSecure('tauri-app', 'refresh-token');
} catch (error) {
  console.error('Tokens not found');
}

// Delete secure values
await deleteSecure('tauri-app', 'oauth-token');
```

### Vue Login Component Example

```vue
<template>
  <div class="login-form">
    <input v-model="username" placeholder="Username" />
    <input v-model="password" type="password" placeholder="Password" />
    <label>
      <input v-model="rememberMe" type="checkbox" />
      Remember me
    </label>
    <button @click="login">Login</button>
  </div>
</template>

<script lang="ts">
import { storeCredentials, getCredentials, deleteCredentials } from '@/utils/storage';

export default {
  data() {
    return {
      username: '',
      password: '',
      rememberMe: false
    };
  },
  
  async mounted() {
    // Try to load saved credentials
    const creds = await getCredentials();
    if (creds) {
      this.username = creds.username;
      this.password = creds.password;
      this.rememberMe = true;
    }
  },
  
  methods: {
    async login() {
      // Perform authentication
      const success = await this.authenticate(this.username, this.password);
      
      if (success) {
        if (this.rememberMe) {
          // Store credentials securely
          await storeCredentials(this.username, this.password);
        } else {
          // Clear any stored credentials
          await deleteCredentials();
        }
        
        // Navigate to dashboard
        this.$router.push('/dashboard');
      }
    },
    
    async authenticate(username: string, password: string): Promise<boolean> {
      // Your authentication logic here
      return true;
    }
  }
};
</script>
```

---

## Real-World Usage Patterns

### Pattern 1: App Configuration Manager

```typescript
// src/utils/config.ts
import { savePreference, getPreference } from './storage';

interface AppConfig {
  theme: 'light' | 'dark';
  language: string;
  fontSize: number;
  autoSave: boolean;
  notifications: boolean;
}

const DEFAULT_CONFIG: AppConfig = {
  theme: 'light',
  language: 'en',
  fontSize: 14,
  autoSave: true,
  notifications: true
};

export async function loadConfig(): Promise<AppConfig> {
  return await getPreference<AppConfig>('app-config', DEFAULT_CONFIG);
}

export async function saveConfig(config: AppConfig): Promise<void> {
  await savePreference('app-config', config);
}

export async function updateConfig(partial: Partial<AppConfig>): Promise<void> {
  const current = await loadConfig();
  const updated = { ...current, ...partial };
  await saveConfig(updated);
}
```

### Pattern 2: Authentication Manager

```typescript
// src/utils/auth.ts
import { storeApiToken, getApiToken, deleteApiToken } from './storage';

export async function login(username: string, password: string): Promise<boolean> {
  try {
    // Call your authentication endpoint
    const response = await fetch('http://localhost:5000/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });
    
    const data = await response.json();
    
    if (data.token) {
      // Store token securely
      await storeApiToken(data.token);
      return true;
    }
    
    return false;
  } catch (error) {
    console.error('Login failed:', error);
    return false;
  }
}

export async function logout(): Promise<void> {
  await deleteApiToken();
}

export async function isAuthenticated(): Promise<boolean> {
  const token = await getApiToken();
  return token !== null;
}

export async function getAuthHeaders(): Promise<Record<string, string>> {
  const token = await getApiToken();
  if (!token) {
    throw new Error('Not authenticated');
  }
  
  return {
    'Authorization': `Bearer ${token}`
  };
}
```

### Pattern 3: Window State Persistence

```typescript
// src/utils/windowState.ts
import { savePreference, getPreference } from './storage';
import { getCurrentWindow } from '@tauri-apps/api/window';

interface WindowState {
  x: number;
  y: number;
  width: number;
  height: number;
  maximized: boolean;
}

export async function saveWindowState(): Promise<void> {
  const window = getCurrentWindow();
  
  const position = await window.outerPosition();
  const size = await window.outerSize();
  const maximized = await window.isMaximized();
  
  const state: WindowState = {
    x: position.x,
    y: position.y,
    width: size.width,
    height: size.height,
    maximized
  };
  
  await savePreference('window-state', state);
}

export async function restoreWindowState(): Promise<void> {
  const state = await getPreference<WindowState>('window-state');
  
  if (state) {
    const window = getCurrentWindow();
    
    if (state.maximized) {
      await window.maximize();
    } else {
      await window.setPosition({ x: state.x, y: state.y });
      await window.setSize({ width: state.width, height: state.height });
    }
  }
}
```

---

## Security Best Practices

### ✅ DO

- **Use secure storage for:**
  - API tokens and OAuth tokens
  - Passwords and credentials
  - Encryption keys
  - Private keys
  - Session tokens
  - Any sensitive user data

- **Use store plugin for:**
  - User preferences (theme, language)
  - UI state (window position, layout)
  - Non-sensitive settings
  - Cache data
  - Application configuration

### ❌ DON'T

- ❌ Don't store passwords in the store plugin (use secure storage)
- ❌ Don't store large binary data in secure storage (use file system)
- ❌ Don't store secure data in localStorage/sessionStorage
- ❌ Don't hardcode sensitive values in source code
- ❌ Don't log secure storage values to console

### Validation Example

```typescript
// Validate sensitive data before storage
function isValidToken(token: string): boolean {
  return token.length > 0 && token.length < 10000;
}

async function storeTokenSafely(token: string): Promise<void> {
  if (!isValidToken(token)) {
    throw new Error('Invalid token format');
  }
  
  await storeApiToken(token);
}
```

---

## Testing Storage

### Manual Testing

#### Test Store Plugin (Windows)
```powershell
# View stored preferences
Get-Content "$env:APPDATA\com.sudipmandal.tauri-app\app-settings.json"
```

#### Test Secure Storage (Windows)
```powershell
# Open Credential Manager
control /name Microsoft.CredentialManager

# Look for entries with "tauri-app" service name
```

#### Test Secure Storage (Linux)
```bash
# Install secret-tool
sudo apt install libsecret-tools

# List all stored credentials
secret-tool search service tauri-app

# Get specific credential
secret-tool lookup service tauri-app username api-token
```

#### Test Secure Storage (macOS)
```bash
# Open Keychain Access
open "/Applications/Utilities/Keychain Access.app"

# Or use command line
security find-generic-password -s "tauri-app" -a "api-token"
```

### Automated Tests

```typescript
// Example test in Vue component
export default {
  methods: {
    async testStorage() {
      console.log('=== Testing Storage ===');
      
      // Test store plugin
      await savePreference('test-key', 'test-value');
      const value = await getPreference('test-key');
      console.log('Store plugin test:', value === 'test-value' ? '✅' : '❌');
      
      // Test secure storage
      try {
        await storeApiToken('test-token-12345');
        const token = await getApiToken();
        console.log('Secure storage test:', token === 'test-token-12345' ? '✅' : '❌');
        
        // Cleanup
        await deleteApiToken();
      } catch (error) {
        console.error('Secure storage test failed:', error);
      }
      
      console.log('=== Tests Complete ===');
    }
  }
};
```

---

## Troubleshooting

### Issue: "Failed to store secure value"

**Windows**: Ensure Windows Credential Manager service is running
```powershell
Get-Service -Name VaultSvc
Start-Service VaultSvc
```

**Linux**: Install and configure keyring
```bash
# Ubuntu/Debian
sudo apt install gnome-keyring libsecret-1-0

# Arch Linux
sudo pacman -S gnome-keyring libsecret
```

**macOS**: Usually works out-of-the-box, ensure Keychain Access is not disabled

### Issue: Store file not being created

Check app data directory exists and has write permissions:
```typescript
// Add this to your app initialization
import { appDataDir } from '@tauri-apps/api/path';

const dataDir = await appDataDir();
console.log('App data directory:', dataDir);
```

### Issue: Credentials persist after uninstall

This is **expected behavior**. Secure credentials are stored in the OS credential manager and persist after app uninstall. To clear them:

**Windows**: `Control Panel > Credential Manager > Windows Credentials` (remove entries starting with "tauri-app")
**macOS**: `Keychain Access.app` (search for "tauri-app")
**Linux**: `secret-tool clear service tauri-app`

---

## Migration Guide

### Migrating from localStorage

```typescript
// Old approach (browser only, not secure)
localStorage.setItem('token', token);
const token = localStorage.getItem('token');

// New approach (cross-platform, secure)
await storeApiToken(token);
const token = await getApiToken();
```

### Migrating from file-based storage

```typescript
// Old approach (manual file management)
import { writeTextFile, readTextFile } from '@tauri-apps/api/fs';

await writeTextFile('config.json', JSON.stringify(config));
const text = await readTextFile('config.json');
const config = JSON.parse(text);

// New approach (managed, automatic persistence)
await savePreference('config', config);
const config = await getPreference('config');
```

---

## API Reference

See `src/utils/storage.ts` for full TypeScript definitions and inline documentation.

### Store Plugin Functions

- `savePreference(key, value)` - Save non-sensitive data
- `getPreference(key, defaultValue?)` - Get non-sensitive data
- `deletePreference(key)` - Delete non-sensitive data
- `getAllPreferenceKeys()` - List all keys
- `clearAllPreferences()` - Clear all non-sensitive data

### Secure Storage Functions

- `storeSecure(service, key, value)` - Store secure value
- `getSecure(service, key)` - Get secure value
- `deleteSecure(service, key)` - Delete secure value
- `storeApiToken(token)` - Store API token (convenience)
- `getApiToken()` - Get API token (convenience)
- `deleteApiToken()` - Delete API token (convenience)
- `storeCredentials(username, password)` - Store credentials (convenience)
- `getCredentials()` - Get credentials (convenience)
- `deleteCredentials()` - Delete credentials (convenience)

---

## Additional Resources

- [Tauri Store Plugin Documentation](https://tauri.app/plugin/store)
- [keyring-rs GitHub](https://github.com/hwchen/keyring-rs)
- [Tauri Security Best Practices](https://tauri.app/security/)

---

## Summary

✅ **Use Store Plugin for**: Preferences, UI state, non-sensitive settings  
✅ **Use Secure Storage for**: Tokens, passwords, API keys, secrets  
✅ **Cross-platform**: Both work on Windows, macOS, and Linux  
✅ **Easy to use**: Simple async/await API  
✅ **Secure by default**: OS-level encryption for sensitive data
