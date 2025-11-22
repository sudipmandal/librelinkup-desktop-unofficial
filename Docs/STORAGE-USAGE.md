# How to Use Storage in Your App

## Quick Integration Guide

### Option 1: Add Storage Demo to App.vue

Add a tab to show the storage demo:

```vue
<template>
  <div id="app">
    <h1>Welcome to Tauri + Vue + C#!</h1>
    
    <!-- Tab Navigation -->
    <div class="tabs">
      <button @click="currentTab = 'demo'" :class="{ active: currentTab === 'demo' }">
        Demo
      </button>
      <button @click="currentTab = 'storage'" :class="{ active: currentTab === 'storage' }">
        Storage
      </button>
    </div>
    
    <!-- Tab Content -->
    <div v-if="currentTab === 'demo'">
      <!-- Your existing demo content -->
    </div>
    
    <div v-if="currentTab === 'storage'">
      <StorageDemo />
    </div>
  </div>
</template>

<script lang="ts">
import StorageDemo from './components/StorageDemo.vue';

export default {
  components: {
    StorageDemo
  },
  data() {
    return {
      currentTab: 'demo'
    };
  }
};
</script>
```

### Option 2: Use Storage Directly in Your Components

```vue
<template>
  <div>
    <h2>User Preferences</h2>
    
    <label>
      Theme:
      <select v-model="theme" @change="saveTheme">
        <option value="light">Light</option>
        <option value="dark">Dark</option>
      </select>
    </label>
    
    <button @click="login">Login</button>
  </div>
</template>

<script lang="ts">
import { savePreference, getPreference, storeApiToken } from '@/utils/storage';

export default {
  data() {
    return {
      theme: 'light'
    };
  },
  
  async mounted() {
    // Load saved theme on startup
    this.theme = await getPreference('theme', 'light');
    this.applyTheme(this.theme);
  },
  
  methods: {
    async saveTheme() {
      await savePreference('theme', this.theme);
      this.applyTheme(this.theme);
    },
    
    applyTheme(theme: string) {
      document.body.className = theme;
    },
    
    async login() {
      // Your login logic here
      const token = await this.authenticate();
      
      if (token) {
        // Store token securely
        await storeApiToken(token);
        this.$router.push('/dashboard');
      }
    }
  }
};
</script>
```

### Option 3: Create an Auth Service

```typescript
// src/services/auth.ts
import { storeApiToken, getApiToken, deleteApiToken } from '@/utils/storage';

export class AuthService {
  private static baseUrl = 'http://localhost:5000/api';
  
  static async login(username: string, password: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });
      
      const data = await response.json();
      
      if (data.token) {
        await storeApiToken(data.token);
        return true;
      }
      
      return false;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    }
  }
  
  static async logout(): Promise<void> {
    await deleteApiToken();
  }
  
  static async isAuthenticated(): Promise<boolean> {
    const token = await getApiToken();
    return token !== null;
  }
  
  static async getAuthHeaders(): Promise<Record<string, string>> {
    const token = await getApiToken();
    if (!token) {
      throw new Error('Not authenticated');
    }
    
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }
  
  static async authenticatedFetch(url: string, options: RequestInit = {}): Promise<Response> {
    const headers = await this.getAuthHeaders();
    
    return fetch(url, {
      ...options,
      headers: {
        ...headers,
        ...options.headers
      }
    });
  }
}
```

Then use it in components:

```vue
<script lang="ts">
import { AuthService } from '@/services/auth';

export default {
  methods: {
    async handleLogin() {
      const success = await AuthService.login(this.username, this.password);
      if (success) {
        this.$router.push('/dashboard');
      }
    },
    
    async fetchUserData() {
      const response = await AuthService.authenticatedFetch(
        'http://localhost:5000/api/user/profile'
      );
      const data = await response.json();
      return data;
    }
  }
};
</script>
```

## Real-World Example: Settings Manager

```typescript
// src/services/settings.ts
import { savePreference, getPreference } from '@/utils/storage';

export interface AppSettings {
  theme: 'light' | 'dark' | 'auto';
  language: string;
  fontSize: number;
  notifications: boolean;
  autoSave: boolean;
  saveInterval: number;
}

const DEFAULT_SETTINGS: AppSettings = {
  theme: 'light',
  language: 'en',
  fontSize: 14,
  notifications: true,
  autoSave: true,
  saveInterval: 30
};

export class SettingsService {
  private static settings: AppSettings | null = null;
  
  static async load(): Promise<AppSettings> {
    if (!this.settings) {
      this.settings = await getPreference('app-settings', DEFAULT_SETTINGS);
    }
    return this.settings;
  }
  
  static async save(settings: AppSettings): Promise<void> {
    this.settings = settings;
    await savePreference('app-settings', settings);
  }
  
  static async update(partial: Partial<AppSettings>): Promise<void> {
    const current = await this.load();
    const updated = { ...current, ...partial };
    await this.save(updated);
  }
  
  static async reset(): Promise<void> {
    await this.save(DEFAULT_SETTINGS);
  }
}
```

Use in component:

```vue
<template>
  <div>
    <h2>Settings</h2>
    
    <label>
      Theme:
      <select v-model="settings.theme" @change="updateSettings">
        <option value="light">Light</option>
        <option value="dark">Dark</option>
        <option value="auto">Auto</option>
      </select>
    </label>
    
    <label>
      Font Size:
      <input type="number" v-model.number="settings.fontSize" @change="updateSettings" />
    </label>
    
    <label>
      <input type="checkbox" v-model="settings.notifications" @change="updateSettings" />
      Enable Notifications
    </label>
    
    <button @click="resetSettings">Reset to Defaults</button>
  </div>
</template>

<script lang="ts">
import { SettingsService, AppSettings } from '@/services/settings';

export default {
  data() {
    return {
      settings: null as AppSettings | null
    };
  },
  
  async mounted() {
    this.settings = await SettingsService.load();
  },
  
  methods: {
    async updateSettings() {
      if (this.settings) {
        await SettingsService.save(this.settings);
      }
    },
    
    async resetSettings() {
      await SettingsService.reset();
      this.settings = await SettingsService.load();
    }
  }
};
</script>
```

## Testing Your Storage

Add this method to any component to test storage:

```typescript
methods: {
  async testStorage() {
    console.log('=== Testing Storage ===');
    
    // Test preferences
    await savePreference('test-key', 'test-value');
    const value = await getPreference('test-key');
    console.log('Store Plugin:', value === 'test-value' ? '✅ PASS' : '❌ FAIL');
    
    // Test secure storage
    await storeApiToken('test-token-123');
    const token = await getApiToken();
    console.log('Secure Storage:', token === 'test-token-123' ? '✅ PASS' : '❌ FAIL');
    
    // Cleanup
    await deleteApiToken();
    console.log('=== Tests Complete ===');
  }
}
```

## Platform-Specific Verification

### Windows
```powershell
# Check store file
Get-Content "$env:APPDATA\com.sudipmandal.tauri-app\app-settings.json"

# Check secure storage
# Open: Control Panel > Credential Manager > Windows Credentials
# Look for: "tauri-app" entries
```

### macOS
```bash
# Check store file
cat "~/Library/Application Support/com.sudipmandal.tauri-app/app-settings.json"

# Check secure storage
open "/Applications/Utilities/Keychain Access.app"
# Search for: "tauri-app"
```

### Linux
```bash
# Check store file
cat "~/.config/com.sudipmandal.tauri-app/app-settings.json"

# Check secure storage
secret-tool search service tauri-app
```

## Best Practices

### ✅ DO

1. **Load settings on app startup**:
   ```typescript
   async mounted() {
     this.settings = await SettingsService.load();
     this.applySettings(this.settings);
   }
   ```

2. **Save settings immediately after changes**:
   ```typescript
   async updateTheme(theme: string) {
     await savePreference('theme', theme);
     this.applyTheme(theme);
   }
   ```

3. **Handle errors gracefully**:
   ```typescript
   try {
     await storeApiToken(token);
   } catch (error) {
     console.error('Failed to save token:', error);
     this.showError('Could not save token securely');
   }
   ```

4. **Provide defaults**:
   ```typescript
   const theme = await getPreference('theme', 'light'); // Default to 'light'
   ```

### ❌ DON'T

1. ❌ Don't store sensitive data in store plugin
2. ❌ Don't store large files (use file system instead)
3. ❌ Don't forget to handle missing values
4. ❌ Don't expose secure values in console.log
5. ❌ Don't assume storage is synchronous (always await)

## Next Steps

1. ✅ Import storage utilities in your components
2. ✅ Add StorageDemo to see it in action
3. ✅ Create settings/auth services for your app
4. ✅ Test on all target platforms
5. ✅ Read STORAGE-GUIDE.md for complete documentation

---

**Your app is now ready for production-grade storage! 🎉**
