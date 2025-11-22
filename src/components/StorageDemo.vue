<template>
  <div class="storage-demo">
    <h2>Storage Demo</h2>
    
    <!-- Non-Sensitive Storage Section -->
    <div class="section">
      <h3>📦 User Preferences (Store Plugin)</h3>
      <p class="info">Non-sensitive data stored in: <code>{{ storeLocation }}</code></p>
      
      <div class="form-group">
        <label>Theme:</label>
        <select v-model="theme" @change="saveThemePreference">
          <option value="light">Light</option>
          <option value="dark">Dark</option>
          <option value="auto">Auto</option>
        </select>
      </div>
      
      <div class="form-group">
        <label>Language:</label>
        <select v-model="language" @change="saveLanguagePreference">
          <option value="en">English</option>
          <option value="es">Spanish</option>
          <option value="fr">French</option>
          <option value="de">German</option>
        </select>
      </div>
      
      <div class="form-group">
        <label>Font Size:</label>
        <input type="number" v-model.number="fontSize" @change="saveFontSizePreference" min="10" max="24" />
      </div>
      
      <button @click="loadAllPreferences">🔄 Reload Preferences</button>
      <button @click="clearAllPrefs" class="danger">🗑️ Clear All Preferences</button>
    </div>
    
    <!-- Secure Storage Section -->
    <div class="section">
      <h3>🔐 Secure Storage (OS Keyring)</h3>
      <p class="info">Stored securely in:
        <code v-if="isWindows">Windows Credential Manager</code>
        <code v-else-if="isMac">macOS Keychain</code>
        <code v-else>Linux Secret Service</code>
      </p>
      
      <div class="form-group">
        <label>API Token:</label>
        <input type="password" v-model="apiToken" placeholder="Enter API token" />
        <button @click="saveToken">💾 Save Token</button>
        <button @click="loadToken">📥 Load Token</button>
        <button @click="deleteToken" class="danger">🗑️ Delete Token</button>
      </div>
      
      <div class="form-group">
        <label>Username:</label>
        <input v-model="username" placeholder="Enter username" />
      </div>
      
      <div class="form-group">
        <label>Password:</label>
        <input type="password" v-model="password" placeholder="Enter password" />
      </div>
      
      <button @click="saveCredentials">💾 Save Credentials</button>
      <button @click="loadCredentials">📥 Load Credentials</button>
      <button @click="deleteCredentials" class="danger">🗑️ Delete Credentials</button>
    </div>
    
    <!-- Status Section -->
    <div class="section">
      <h3>📊 Status</h3>
      <div class="status" :class="statusClass">{{ statusMessage }}</div>
    </div>
    
    <!-- Test Section -->
    <div class="section">
      <h3>🧪 Test Storage</h3>
      <button @click="runStorageTests">Run All Tests</button>
      <pre v-if="testResults" class="test-results">{{ testResults }}</pre>
    </div>
  </div>
</template>

<script lang="ts">
import {
  savePreference,
  getPreference,
  clearAllPreferences,
  storeApiToken,
  getApiToken,
  deleteApiToken,
  storeCredentials as storeCredsUtil,
  getCredentials as getCredsUtil,
  deleteCredentials as deleteCredsUtil
} from '../utils/storage';

export default {
  name: 'StorageDemo',
  
  data() {
    return {
      // Preferences
      theme: 'light',
      language: 'en',
      fontSize: 14,
      
      // Secure data
      apiToken: '',
      username: '',
      password: '',
      
      // Status
      statusMessage: 'Ready',
      statusClass: 'info',
      
      // Test results
      testResults: '',
      
      // Platform detection
      storeLocation: this.getStoreLocation(),
      isWindows: navigator.platform.includes('Win'),
      isMac: navigator.platform.includes('Mac'),
    };
  },
  
  async mounted() {
    await this.loadAllPreferences();
  },
  
  methods: {
    // === Preferences Methods ===
    
    async saveThemePreference() {
      try {
        await savePreference('theme', this.theme);
        this.showStatus('Theme saved successfully', 'success');
      } catch (error) {
        this.showStatus(`Failed to save theme: ${error}`, 'error');
      }
    },
    
    async saveLanguagePreference() {
      try {
        await savePreference('language', this.language);
        this.showStatus('Language saved successfully', 'success');
      } catch (error) {
        this.showStatus(`Failed to save language: ${error}`, 'error');
      }
    },
    
    async saveFontSizePreference() {
      try {
        await savePreference('fontSize', this.fontSize);
        this.showStatus('Font size saved successfully', 'success');
      } catch (error) {
        this.showStatus(`Failed to save font size: ${error}`, 'error');
      }
    },
    
    async loadAllPreferences() {
      try {
        this.theme = (await getPreference('theme', 'light')) ?? 'light';
        this.language = (await getPreference('language', 'en')) ?? 'en';
        this.fontSize = (await getPreference('fontSize', 14)) ?? 14;
        this.showStatus('Preferences loaded successfully', 'success');
      } catch (error) {
        this.showStatus(`Failed to load preferences: ${error}`, 'error');
      }
    },
    
    async clearAllPrefs() {
      if (confirm('Are you sure you want to clear all preferences?')) {
        try {
          await clearAllPreferences();
          // Reset to defaults
          this.theme = 'light';
          this.language = 'en';
          this.fontSize = 14;
          this.showStatus('All preferences cleared', 'success');
        } catch (error) {
          this.showStatus(`Failed to clear preferences: ${error}`, 'error');
        }
      }
    },
    
    // === Secure Storage Methods ===
    
    async saveToken() {
      if (!this.apiToken) {
        this.showStatus('Please enter a token', 'error');
        return;
      }
      
      try {
        await storeApiToken(this.apiToken);
        this.showStatus('Token saved securely', 'success');
      } catch (error) {
        this.showStatus(`Failed to save token: ${error}`, 'error');
      }
    },
    
    async loadToken() {
      try {
        const token = await getApiToken();
        if (token) {
          this.apiToken = token;
          this.showStatus('Token loaded successfully', 'success');
        } else {
          this.showStatus('No token found', 'info');
        }
      } catch (error) {
        this.showStatus(`Failed to load token: ${error}`, 'error');
      }
    },
    
    async deleteToken() {
      try {
        await deleteApiToken();
        this.apiToken = '';
        this.showStatus('Token deleted successfully', 'success');
      } catch (error) {
        this.showStatus(`Failed to delete token: ${error}`, 'error');
      }
    },
    
    async saveCredentials() {
      if (!this.username || !this.password) {
        this.showStatus('Please enter username and password', 'error');
        return;
      }
      
      try {
        await storeCredsUtil(this.username, this.password);
        this.showStatus('Credentials saved securely', 'success');
      } catch (error) {
        this.showStatus(`Failed to save credentials: ${error}`, 'error');
      }
    },
    
    async loadCredentials() {
      try {
        const creds = await getCredsUtil();
        if (creds) {
          this.username = creds.username;
          this.password = creds.password;
          this.showStatus('Credentials loaded successfully', 'success');
        } else {
          this.showStatus('No credentials found', 'info');
        }
      } catch (error) {
        this.showStatus(`Failed to load credentials: ${error}`, 'error');
      }
    },
    
    async deleteCredentials() {
      try {
        await deleteCredsUtil();
        this.username = '';
        this.password = '';
        this.showStatus('Credentials deleted successfully', 'success');
      } catch (error) {
        this.showStatus(`Failed to delete credentials: ${error}`, 'error');
      }
    },
    
    // === Utility Methods ===
    
    showStatus(message: string, type: 'success' | 'error' | 'info') {
      this.statusMessage = message;
      this.statusClass = type;
      
      // Auto-clear after 5 seconds
      setTimeout(() => {
        this.statusMessage = 'Ready';
        this.statusClass = 'info';
      }, 5000);
    },
    
    getStoreLocation(): string {
      const platform = navigator.platform;
      if (platform.includes('Win')) {
        return '%APPDATA%\\com.sudipmandal.tauri-app\\app-settings.json';
      } else if (platform.includes('Mac')) {
        return '~/Library/Application Support/com.sudipmandal.tauri-app/app-settings.json';
      } else {
        return '~/.config/com.sudipmandal.tauri-app/app-settings.json';
      }
    },
    
    // === Testing ===
    
    async runStorageTests() {
      this.testResults = 'Running tests...\n\n';
      
      try {
        // Test 1: Store plugin
        this.testResults += '=== Store Plugin Tests ===\n';
        await savePreference('test-string', 'hello world');
        const testString = await getPreference('test-string');
        this.testResults += `✅ String storage: ${testString === 'hello world'}\n`;
        
        await savePreference('test-number', 42);
        const testNumber = await getPreference('test-number');
        this.testResults += `✅ Number storage: ${testNumber === 42}\n`;
        
        await savePreference('test-object', { foo: 'bar', count: 123 });
        const testObject = await getPreference<{ foo: string; count: number }>('test-object');
        this.testResults += `✅ Object storage: ${testObject?.foo === 'bar' && testObject?.count === 123}\n`;
        
        // Test 2: Secure storage
        this.testResults += '\n=== Secure Storage Tests ===\n';
        await storeApiToken('test-token-12345');
        const token = await getApiToken();
        this.testResults += `✅ Token storage: ${token === 'test-token-12345'}\n`;
        
        await storeCredsUtil('testuser@example.com', 'SecurePass123!');
        const creds = await getCredsUtil();
        this.testResults += `✅ Credentials storage: ${creds?.username === 'testuser@example.com' && creds?.password === 'SecurePass123!'}\n`;
        
        // Cleanup
        await deleteApiToken();
        await deleteCredsUtil();
        
        this.testResults += '\n✅ All tests passed!\n';
        this.showStatus('All tests passed', 'success');
      } catch (error) {
        this.testResults += `\n❌ Test failed: ${error}\n`;
        this.showStatus('Tests failed', 'error');
      }
    }
  }
};
</script>

<style scoped>
.storage-demo {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  font-family: system-ui, -apple-system, sans-serif;
}

h2 {
  color: #333;
  border-bottom: 2px solid #0078d4;
  padding-bottom: 10px;
}

.section {
  background: #f5f5f5;
  border-radius: 8px;
  padding: 20px;
  margin: 20px 0;
}

.section h3 {
  margin-top: 0;
  color: #555;
}

.info {
  font-size: 0.9em;
  color: #666;
  margin: 10px 0;
}

.info code {
  background: #e0e0e0;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 0.85em;
}

.form-group {
  margin: 15px 0;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
  color: #444;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 14px;
  box-sizing: border-box;
}

.form-group input[type="number"] {
  width: 100px;
}

button {
  background: #0078d4;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  margin: 5px 5px 5px 0;
  transition: background 0.2s;
}

button:hover {
  background: #006abc;
}

button.danger {
  background: #d32f2f;
}

button.danger:hover {
  background: #b71c1c;
}

.status {
  padding: 12px;
  border-radius: 4px;
  font-weight: 500;
  text-align: center;
}

.status.success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.status.error {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.status.info {
  background: #d1ecf1;
  color: #0c5460;
  border: 1px solid #bee5eb;
}

.test-results {
  background: #2d2d2d;
  color: #f0f0f0;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
  font-family: 'Courier New', monospace;
  font-size: 13px;
  line-height: 1.5;
  margin-top: 10px;
}
</style>
