<script lang="ts">
import { defineComponent, markRaw } from "vue";
import { getCurrentWindow, LogicalSize } from "@tauri-apps/api/window";
import { getAuthToken, getCGMData } from "./lib/linkup";

export default defineComponent({
  name: "App",
  data() {
    return {
      country: "global",
      email: "",
      password: "",
      alwaysOnTop: false,
      alwaysOnTopInterval: null as number | null,
      refreshInterval: null as number | null,
      store: null as any,
      storeReady: false,
      isLoading: false,
      isLoggedIn: false,
      cgmData: null as any
    };
  },
  computed: {
    isLoginDisabled(): boolean {
      return !this.email || !this.password;
    },
    glucoseBackgroundColor(): string {
      if (!this.cgmData?.glucoseMeasurement?.Value) return '#e81123';
      const value = this.cgmData.glucoseMeasurement.Value;
      
      if (value >= 4.2 && value <= 10) {
        return '#28a745'; // Green
      } else if (value >= 10.1 && value <= 13.9) {
        return '#fd7e14'; // Orange
      } else {
        return '#e81123'; // Red
      }
    }
  },

  methods: {
    async minimizeWindow() {
      await getCurrentWindow().minimize();
    },
    async maximizeWindow() {
      await getCurrentWindow().toggleMaximize();
    },
    async closeWindow() {
      await getCurrentWindow().close();
    },
    async handleLogin() {
      console.log("Login attempt:", {
        country: this.country,
        email: this.email,
        password: "***"
      });
      
      // Save form values first
      await this.saveFormValues();
      
      try {
        this.isLoading = true;
        
        // Call LibreLinkUp API directly using axios
        const result = await getAuthToken({
          country: this.country,
          username: this.email,
          password: this.password
        });
        
        if (!result || 'error' in result) {
          console.error("Login failed:", result);
          // Reset to login form on error
          this.isLoggedIn = false;
          this.cgmData = null;
          return;
        }
        
        console.log("Login successful:", result);
        
        // Store the token and account info
        await this.store.set("token", result.token);
        await this.store.set("accountId", result.accountId);
        await this.store.set("accountCountry", result.accountCountry);
        await this.store.save();
        
        // Fetch CGM data
        await this.fetchCGMData();
        
      } catch (error) {
        console.error("Login error:", error);
        // Reset to login form on error
        this.isLoggedIn = false;
        this.cgmData = null;
      } finally {
        this.isLoading = false;
      }
    },
    async fetchCGMData() {
      try {
        const token = await this.store.get("token");
        const accountId = await this.store.get("accountId");
        const accountCountry = await this.store.get("accountCountry");
        
        if (!token || !accountId) {
          console.error("Missing token or account ID");
          return;
        }
        
        // Call LibreLinkUp API directly using axios
        const data = await getCGMData({
          token,
          country: accountCountry || this.country,
          accountId
        });
        
        if (!data || (typeof data === 'object' && 'error' in data)) {
          console.error("Failed to fetch CGM data:", data);
          // Reset to login form on error
          await this.handleError();
          return;
        }
        
        console.log("CGM data:", data);
        this.cgmData = data;
        this.isLoggedIn = true;
        
        // Resize window to compact size
        const window = getCurrentWindow();
        await window.setResizable(true);
        await window.setSize(new LogicalSize(249, 58));
        
        // Set up auto-refresh every 30 seconds
        if (!this.refreshInterval) {
          this.refreshInterval = setInterval(() => {
            this.fetchCGMData();
          }, 30000);
        }
        
      } catch (error) {
        console.error("Error fetching CGM data:", error);
        // Reset to login form on error
        await this.handleError();
      }
    },
    async handleError() {
      // Clear auto-refresh interval
      if (this.refreshInterval) {
        clearInterval(this.refreshInterval);
        this.refreshInterval = null;
      }
      
      // Reset to login state
      this.isLoggedIn = false;
      this.cgmData = null;
      
      // Resize window back to login form size
      const window = getCurrentWindow();
      try {
        await window.setResizable(true);
        await window.setSize(new LogicalSize(800, 600));
      } catch (error) {
        console.error("Failed to resize window:", error);
      }
    },
    async logout() {
      // Clear auto-refresh interval
      if (this.refreshInterval) {
        clearInterval(this.refreshInterval);
        this.refreshInterval = null;
      }
      
      // Resize window back to original size first, before changing state
      const window = getCurrentWindow();
      try {
        await window.setResizable(true);
        await window.setSize(new LogicalSize(800, 600));
      } catch (error) {
        console.error("Failed to resize window:", error);
      }
      
      // Then update state
      this.isLoggedIn = false;
      this.cgmData = null;
    },
    getTrendArrow(trend: number): string {
      const arrows: Record<number, string> = {
        1: '↓',   // Falling
        2: '↓↓',  // Falling quickly
        3: '→',   // Steady
        4: '↑',  // Rising
        5: '↑↑',   // Rising quickly
      };
      return arrows[trend] || '?';
    },
    formatTimestamp(timestamp: string): string {
      const date = new Date(timestamp);
      return date.toLocaleString();
    },
    async saveFormValues() {
      if (this.store && this.storeReady && !this.isLoading) {
        try {
          await this.store.set("country", this.country);
          await this.store.set("email", this.email);
          await this.store.set("password", this.password);
          await this.store.set("alwaysOnTop", this.alwaysOnTop);
          await this.store.save();
          
          console.log("Form values saved:", { country: this.country, email: this.email, alwaysOnTop: this.alwaysOnTop, hasPassword: !!this.password });
        } catch (error) {
          console.error("Failed to save form values:", error);
        }
      }
    },
    async loadFormValues() {
      if (this.store) {
        this.isLoading = true;
        try {
          const savedCountry = await this.store.get("country");
          const savedEmail = await this.store.get("email");
          const savedPassword = await this.store.get("password");
          const savedAlwaysOnTop = await this.store.get("alwaysOnTop");
          
          console.log("Loading saved values:", { savedCountry, savedEmail, savedAlwaysOnTop, hasPassword: !!savedPassword });
          
          if (savedCountry) this.country = savedCountry as string;
          if (savedEmail) this.email = savedEmail as string;
          if (savedPassword) this.password = savedPassword as string;
          if (savedAlwaysOnTop !== null && savedAlwaysOnTop !== undefined) {
            this.alwaysOnTop = savedAlwaysOnTop as boolean;
          }
          
          // If always on top was enabled, ensure window stays on top
          if (this.alwaysOnTop) {
            const window = getCurrentWindow();
            await window.setAlwaysOnTop(true);
            console.log("Always on top enforced on load");
            
            // Set up periodic enforcement
            this.alwaysOnTopInterval = setInterval(async () => {
              try {
                await window.setAlwaysOnTop(true);
                console.log("Always on top re-enforced");
              } catch (error) {
                console.error("Failed to enforce always on top:", error);
              }
            }, 1000);
          }
          
          console.log("Form values loaded successfully. Password field:", this.password ? "filled" : "empty");
          
          // Attempt auto-login if credentials exist
          if (this.email && this.password) {
            console.log("Auto-login: credentials found, attempting login...");
            await this.handleLogin();
          }
        } catch (error) {
          console.error("Failed to load form values:", error);
        } finally {
          this.isLoading = false;
          this.storeReady = true;
        }
      }
    },
    async toggleAlwaysOnTop() {
      try {
        const window = getCurrentWindow();
        console.log("Setting always on top to:", this.alwaysOnTop);
        await window.setAlwaysOnTop(this.alwaysOnTop);
        
        // Clear existing interval
        if (this.alwaysOnTopInterval) {
          clearInterval(this.alwaysOnTopInterval);
          this.alwaysOnTopInterval = null;
        }
        
        // Set up aggressive enforcement if enabled
        if (this.alwaysOnTop) {
          // Immediate enforcement
          await window.setAlwaysOnTop(true);
          
          // Periodic enforcement
          this.alwaysOnTopInterval = setInterval(async () => {
            try {
              await window.setAlwaysOnTop(true);
              console.log("Always on top re-enforced");
            } catch (error) {
              console.error("Failed to enforce always on top:", error);
            }
          }, 1000); // Check every 1 second
        }
      } catch (error) {
        console.error("Failed to set always on top:", error);
      }
    },
    async enforceAlwaysOnTop() {
      if (this.alwaysOnTop) {
        try {
          await getCurrentWindow().setAlwaysOnTop(true);
          console.log("Always on top enforced on focus");
        } catch (error) {
          console.error("Failed to enforce always on top:", error);
        }
      }
    }
  },
  async mounted() {
    // Initialize store
    try {
      console.log("Initializing store...");
      const { Store } = await import("@tauri-apps/plugin-store");
      const storeInstance = await Store.load("settings.json");
      // Use markRaw to prevent Vue from making the store reactive
      // This prevents "Cannot read private member" errors
      this.store = markRaw(storeInstance);
      console.log("Store initialized, loading values...");
      await this.loadFormValues();
    } catch (error) {
      console.error("Failed to initialize store:", error);
      this.storeReady = true; // Allow saving even if initialization failed
    }
    
    // Set up window focus listener to enforce always on top
    const unlisten = getCurrentWindow().onFocusChanged(({ payload: focused }) => {
      if (focused && this.alwaysOnTop) {
        this.enforceAlwaysOnTop();
      }
    });
    
    // Store unlisten function for cleanup
    (this as any)._unlistenFocus = unlisten;
  },
  async beforeUnmount() {
    // Clean up intervals on component unmount
    if (this.alwaysOnTopInterval) {
      clearInterval(this.alwaysOnTopInterval);
    }
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    
    // Clean up focus listener
    if ((this as any)._unlistenFocus) {
      const unlisten = await (this as any)._unlistenFocus;
      unlisten();
    }
  }
});
</script>

<template>
  <div class="app-window" data-tauri-drag-region>
    <!-- <div class="titlebar" data-tauri-drag-region>
      <div class="titlebar-title">LibreLinkup Desktop - Unofficial</div>
      <div class="titlebar-buttons" style="-webkit-app-region: no-drag;">
        <button class="titlebar-button" @click="minimizeWindow" title="Minimize">
          <span>─</span>
        </button>
        <button class="titlebar-button" @click="maximizeWindow" title="Maximize">
          <span>□</span>
        </button>
        <button class="titlebar-button close" @click="closeWindow" title="Close">
          <span>✕</span>
        </button>
      </div>
    </div> -->
    
    <main class="container">
      <!-- Login Form -->
      <div v-if="!isLoggedIn">
        <form class="login-form" @submit.prevent="handleLogin">
          <div class="form-row">
            <div class="form-group">
              <label for="country">Country</label>
              <select id="country" v-model="country" required>
                <option value="global">Global</option>
                <option value="de">Germany</option>
                <option value="eu">European Union</option>
                <option value="eu2">European Union 2</option>
                <option value="us">United States</option>
                <option value="ap">Asia/Pacific</option>
                <option value="ca">Canada</option>
                <option value="jp">Japan</option>
                <option value="ae">United Arab Emirates</option>
                <option value="fr">France</option>
                <option value="au">Australia</option>
              </select>
            </div>
            
            <div class="form-group checkbox-group">
              <label class="checkbox-label">
                <input 
                  type="checkbox" 
                  v-model="alwaysOnTop" 
                  @change="toggleAlwaysOnTop"
                />
                <span>Always on Top</span>
              </label>
            </div>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label for="email">Email</label>
              <input 
                id="email" 
                type="email" 
                v-model="email" 
                placeholder="Enter your email"
                required 
              />
            </div>
            
            <div class="form-group">
              <label for="password">Password</label>
              <input 
                id="password" 
                type="password" 
                v-model="password" 
                placeholder="Enter your password"
                required 
              />
            </div>
          </div>
        </form>
        
        <div class="button-container">
          <button @click="handleLogin" class="btn-login" :disabled="isLoginDisabled || isLoading">
            {{ isLoading ? 'Logging in...' : 'Login' }}
          </button>
        </div>
      </div>

      <!-- CGM Data Display -->
      <div v-else class="cgm-container" :style="{ background: glucoseBackgroundColor }">
        <button @click="logout" class="btn-logout-gear" title="Logout">
          <span>⚙</span>
        </button>
        
        <div v-if="cgmData?.glucoseMeasurement" class="glucose-display" :style="{ background: glucoseBackgroundColor }">
          <div class="glucose-time">
            {{ formatTimestamp(cgmData.glucoseMeasurement.Timestamp) }}
          </div>
          <div class="glucose-value">
            {{ cgmData.glucoseMeasurement.Value }}
            <span class="glucose-unit">{{ cgmData.glucoseMeasurement.ValueInMgPerDl ? 'mmol/L' : 'mg/dL'  }}</span>
            <span class="glucose-trend" v-if="cgmData.glucoseMeasurement.TrendArrow">{{ getTrendArrow(cgmData.glucoseMeasurement.TrendArrow) }}</span>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

:root {
  font-family: Inter, Avenir, Helvetica, Arial, sans-serif;
  font-size: 16px;
  line-height: 24px;
  font-weight: 400;

  color: #0f0f0f;
  background-color: transparent;

  font-synthesis: none;
  text-rendering: optimizeLegibility;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  -webkit-text-size-adjust: 100%;
}

body {
  background: #f6f6f6;
  overflow: hidden;
}

.app-window {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #f6f6f6;
  overflow: hidden;
  app-region: drag;
}

.titlebar {
  height: 40px;
  background: linear-gradient(180deg, #ffffff 0%, #f0f0f0 100%);
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 16px;
  user-select: none;
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  flex-shrink: 0;
  -webkit-app-region: drag;
  app-region: drag;
}

.titlebar-title {
  font-size: 13px;
  font-weight: 600;
  color: #333;
  pointer-events: none;
}

.titlebar-buttons {
  display: flex;
  gap: 8px;
  margin-left: auto;
}

.titlebar-button {
  width: 32px;
  height: 32px;
  border: none;
  background: transparent;
  -webkit-app-region: no-drag;
  color: #666;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  transition: background-color 0.2s, color 0.2s;
}

.titlebar-button:hover {
  background: rgba(0, 0, 0, 0.05);
  color: #333;
}

.titlebar-button.close:hover {
  background: #e81123;
  color: white;
}

.container {
  flex: 1;
  padding: 0px;
  display: flex;
  flex-direction: column;
  overflow-y: hidden;
}

.login-form {
  max-width: 800px;
  margin: 0 auto;
  padding: 30px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.checkbox-group {
  justify-content: center;
  align-items: center;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  user-select: none;
}

.checkbox-label input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
  margin: 0;
}

.checkbox-label span {
  font-size: 14px;
  font-weight: 500;
}

.form-group label {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 8px;
  color: #333;
}

.form-group input,
.form-group select {
  padding: 12px 16px;
  font-size: 14px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #fff;
  color: #333;
  transition: border-color 0.2s, box-shadow 0.2s;
  font-family: inherit;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #0078d4;
  box-shadow: 0 0 0 3px rgba(0, 120, 212, 0.1);
}

.form-group select {
  cursor: pointer;
}

.button-container {
  display: flex;
  justify-content: center;
  margin-top: 30px;
}

.btn-login {
  padding: 12px 48px;
  font-size: 14px;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  background: linear-gradient(135deg, #0078d4 0%, #0063b1 100%);
  color: white;
  cursor: pointer;
  transition: transform 0.1s, box-shadow 0.2s;
  box-shadow: 0 2px 4px rgba(0, 120, 212, 0.2);
  min-width: 150px;
}

.btn-login:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 120, 212, 0.3);
}

.btn-login:active {
  transform: translateY(0);
}

.btn-login:disabled {
  background: linear-gradient(135deg, #ccc 0%, #999 100%);
  cursor: not-allowed;
  opacity: 0.6;
}

.btn-login:disabled:hover {
  transform: none;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

@media (prefers-color-scheme: dark) {
  :root {
    color: #f6f6f6;
  }

  .app-window {
    background: #2f2f2f;
  }

  .titlebar {
    background: linear-gradient(180deg, #3a3a3a 0%, #2f2f2f 100%);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }

  .titlebar-title {
    color: #f6f6f6;
  }

  .titlebar-button {
    color: #ccc;
  }

  .titlebar-button:hover {
    background: rgba(255, 255, 255, 0.1);
    color: #fff;
  }

  .titlebar-button.close:hover {
    background: #e81123;
    color: white;
  }

  .login-form {
    background: #1e1e1e;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.4);
  }

  .form-group label {
    color: #f6f6f6;
  }

  .form-group input,
  .form-group select {
    background: #2f2f2f;
    border-color: #444;
    color: #f6f6f6;
  }

  .form-group input:focus,
  .form-group select:focus {
    border-color: #0078d4;
    background: #333;
  }

  .cgm-container {
    background: #1e1e1e;
  }

  .btn-logout-gear {
    background: rgba(30, 30, 30, 0.9);
    border-color: #444;
  }

  .btn-logout-gear span {
    color: #ccc;
  }

  .sensor-info {
    background: #2f2f2f;
    color: #ccc;
  }
}

/* CGM Data Display Styles */
.cgm-container {
  padding: 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  height: 100%;
  flex: 1;
}

.btn-logout {
  padding: 8px 16px;
  background: #e81123;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-logout:hover {
  background: #c00a1a;
  transform: translateY(-1px);
}

.btn-logout-gear {
  position: fixed;
  top: 5px;
  right: 5px;
  width: 24px;
  height: 24px;
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid #ddd;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  z-index: 1000;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.btn-logout-gear span {
  font-size: 24px;
  color: #666;
  line-height: 1;
}

.btn-logout-gear:hover {
  background: rgba(255, 255, 255, 1);
  transform: rotate(90deg) scale(1.1);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.glucose-display {
  color: white;
  padding: 5px;
  border-radius: 12px;
  margin-bottom: 0px;
  transition: background 0.3s ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
}

.glucose-value {
  font-size: 28px;
  font-weight: bold;
  line-height: 1;
  display: flex;
  align-items: center;
  gap: 15px;
}

.glucose-unit {
  font-size: 18px;
  font-weight: normal;
  opacity: 0.9;
  display: inline-flex;
  align-items: center;
}

.glucose-trend {
  font-size: 28px;
  display: inline-flex;
  align-items: center;
}

.glucose-time {
  font-size: 12px;
  opacity: 0.7;
  position: absolute;
  top: 2px;
  left: 10px;
}

.sensor-info {
  background: #f6f6f6;
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.sensor-info p {
  margin: 5px 0;
  color: #555;
}

.actions {
  display: flex;
  justify-content: center;
}

.btn-refresh {
  padding: 12px 32px;
  background: linear-gradient(135deg, #0078d4 0%, #005a9e 100%);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  font-weight: 600;
  transition: all 0.2s ease;
}

.btn-refresh:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 120, 212, 0.3);
}

.btn-refresh:disabled {
  background: linear-gradient(135deg, #ccc 0%, #999 100%);
  cursor: not-allowed;
  opacity: 0.6;
}

.btn-refresh:disabled:hover {
  transform: none;
  box-shadow: none;
}
</style>