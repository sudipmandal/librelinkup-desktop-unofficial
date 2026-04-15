<script lang="ts">
import { defineComponent, markRaw } from "vue";
import { getCurrentWindow, LogicalSize } from "@tauri-apps/api/window";
import { invoke } from "@tauri-apps/api/core";
import { getAuthToken, getCGMData } from "../lib/linkup";
import LoginForm from "./LoginForm.vue";
import CgmDisplay from "./CgmDisplay.vue";

export default defineComponent({
  name: "AppSession",
  components: {
    LoginForm,
    CgmDisplay
  },
  data() {
    return {
      country: "global",
      email: "",
      password: "",
      alwaysOnTop: false,
      alwaysOnTopInterval: null as number | null,
      refreshInterval: null as number | null,
      retryInterval: null as number | null,
      store: null as any,
      storeReady: false,
      isLoading: false,
      isLoggedIn: false,
      cgmData: null as any,
      errorMessage: "",
      unlistenFocusPromise: null as Promise<() => void> | null,
    };
  },
  computed: {
    isLoginDisabled(): boolean {
      return !this.email || !this.password;
    },
    glucoseBackgroundColor(): string {
      if (!this.cgmData?.glucoseMeasurement?.Value) return "#e81123";
      const value = this.cgmData.glucoseMeasurement.Value;

      if (value >= 4.2 && value <= 10) {
        return "#28a745";
      } else if (value >= 10.1 && value <= 13.9) {
        return "#fd7e14";
      }

      return "#e81123";
    }
  },
  methods: {
    async resizeWindow(width: number, height: number) {
      const window = getCurrentWindow();
      try {
        await window.setResizable(true);
        await window.setSize(new LogicalSize(width, height));
      } catch (error) {
        console.error("Failed to resize window:", error);
      }
    },
    async handleLogin() {
      console.log(`Login attempt: country=${this.country}, email=${this.email}`);

      this.errorMessage = "";
      if (this.retryInterval) {
        clearInterval(this.retryInterval);
        this.retryInterval = null;
      }

      console.log("Saving form values...");
      await this.saveFormValues();

      const LOGIN_TIMEOUT = 30000;
      let timeoutId: number | null = null;

      try {
        this.isLoading = true;

        const timeoutPromise = new Promise<never>((_, reject) => {
          timeoutId = setTimeout(
            () => reject(new Error("Login timed out after 30 seconds")),
            LOGIN_TIMEOUT
          ) as unknown as number;
        });

        console.log("Calling getAuthToken...");
        const result = await Promise.race([
          getAuthToken({
            country: this.country,
            username: this.email,
            password: this.password
          }),
          timeoutPromise
        ]);

        if (timeoutId) clearTimeout(timeoutId);

        if (!result || "error" in result) {
          console.log(`Login failed: ${JSON.stringify(result)}`);
          const errorMsg = (result as any)?.error || "Login failed. Please check your credentials.";
          await this.handleError(errorMsg);
          return;
        }

        console.log(`Login successful, country=${result.accountCountry}`);

        await this.store.set("token", result.token);
        await this.store.set("accountId", result.accountId);
        await this.store.set("accountCountry", result.accountCountry);
        await this.store.save();
        console.log("Token stored, fetching CGM data...");

        await this.fetchCGMData();
      } catch (error) {
        if (timeoutId) clearTimeout(timeoutId);
        const errorMsg = error instanceof Error ? error.message : "An unexpected error occurred during login.";
        console.log(`Login error: ${errorMsg}`);
        await this.handleError(errorMsg);
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
          console.log("Missing token or account ID");
          return;
        }

        console.log(`Fetching CGM data for country=${accountCountry}...`);

        const data = await getCGMData({
          token,
          country: accountCountry || this.country,
          accountId
        });

        if (!data || (typeof data === "object" && "error" in data)) {
          console.log(`Failed to fetch CGM data: ${JSON.stringify(data)}`);
          const errorMsg = (data as any)?.error || "Failed to fetch CGM data. Retrying...";
          await this.handleError(errorMsg);
          return;
        }

        console.log("CGM data received successfully");
        this.cgmData = data;
        this.isLoggedIn = true;

        await this.resizeWindow(200, 48);

        if (!this.refreshInterval) {
          this.refreshInterval = setInterval(() => {
            this.fetchCGMData();
          }, 30000);
        }
      } catch (error) {
        const errorMsg = error instanceof Error ? error.message : "Failed to fetch CGM data. Retrying...";
        console.log(`CGM data error: ${errorMsg}`);
        await this.handleError(errorMsg);
      }
    },
    async handleError(errorMessage: string = "An error occurred. Retrying...") {
      console.log(`Error: ${errorMessage}`);
      this.errorMessage = errorMessage;

      if (this.refreshInterval) {
        clearInterval(this.refreshInterval);
        this.refreshInterval = null;
      }

      this.isLoggedIn = false;
      this.cgmData = null;

      await this.resizeWindow(800, 600);

      if (!this.retryInterval) {
        console.log("Setting up auto-retry every 10 seconds...");
        this.retryInterval = setInterval(() => {
          console.log("Auto-retrying login...");
          this.handleLogin();
        }, 10000);
      }
    },
    async logout() {
      this.errorMessage = "";
      if (this.retryInterval) {
        clearInterval(this.retryInterval);
        this.retryInterval = null;
      }

      if (this.refreshInterval) {
        clearInterval(this.refreshInterval);
        this.refreshInterval = null;
      }

      await this.resizeWindow(800, 600);

      this.isLoggedIn = false;
      this.cgmData = null;
    },
    async saveFormValues() {
      if (this.store && this.storeReady && !this.isLoading) {
        try {
          await this.store.set("country", this.country);
          await this.store.set("email", this.email);
          await this.store.set("password", this.password);
          await this.store.set("alwaysOnTop", this.alwaysOnTop);
          await this.store.save();

          console.log("Form values saved:", {
            country: this.country,
            email: this.email,
            alwaysOnTop: this.alwaysOnTop,
            hasPassword: !!this.password
          });
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

          console.log("Loading saved values:", {
            savedCountry,
            savedEmail,
            savedAlwaysOnTop,
            hasPassword: !!savedPassword
          });

          if (savedCountry) this.country = savedCountry as string;
          if (savedEmail) this.email = savedEmail as string;
          if (savedPassword) this.password = savedPassword as string;
          if (savedAlwaysOnTop !== null && savedAlwaysOnTop !== undefined) {
            this.alwaysOnTop = savedAlwaysOnTop as boolean;
          }

          if (this.alwaysOnTop) {
            const window = getCurrentWindow();
            await window.setAlwaysOnTop(true);
            console.log("Always on top enforced on load");

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

        invoke<string>("kde_set_always_on_top", {
          enable: this.alwaysOnTop
        }).then((result) => {
          console.log("KDE always-on-top result:", result);
        }).catch((kdeError) => {
          console.log("KDE-specific method not available or failed:", kdeError);
        });

        invoke<string>("gnome_set_always_on_top", {
          enable: this.alwaysOnTop
        }).then((result) => {
          console.log("GNOME always-on-top result:", result);
        }).catch((gnomeError) => {
          console.log("GNOME-specific method not available or failed:", gnomeError);
        });

        await window.setAlwaysOnTop(this.alwaysOnTop);

        if (this.alwaysOnTopInterval) {
          clearInterval(this.alwaysOnTopInterval);
          this.alwaysOnTopInterval = null;
        }

        if (this.alwaysOnTop) {
          await window.setAlwaysOnTop(true);

          this.alwaysOnTopInterval = setInterval(async () => {
            try {
              await window.setAlwaysOnTop(true);
              console.log("Always on top re-enforced");
            } catch (error) {
              console.error("Failed to enforce always on top:", error);
            }
          }, 1000);
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
    try {
      const desktopInfo = await invoke<string>("get_desktop_environment");
      console.log(`Desktop: ${desktopInfo}`);
    } catch (error) {
      console.log(`Desktop env detection failed: ${error}`);
    }

    try {
      console.log("Initializing store...");
      const { Store } = await import("@tauri-apps/plugin-store");
      const storeInstance = await Store.load("settings.json");
      this.store = markRaw(storeInstance);
      console.log("Store initialized, loading values...");
      await this.loadFormValues();
    } catch (error) {
      console.log(`Store init failed: ${error}`);
      this.storeReady = true;
    }

    this.unlistenFocusPromise = getCurrentWindow().onFocusChanged(({ payload: focused }) => {
      if (focused && this.alwaysOnTop) {
        this.enforceAlwaysOnTop();
      }
    });
  },
  async beforeUnmount() {
    if (this.alwaysOnTopInterval) {
      clearInterval(this.alwaysOnTopInterval);
    }
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    if (this.retryInterval) {
      clearInterval(this.retryInterval);
    }

    if (this.unlistenFocusPromise) {
      const unlisten = await this.unlistenFocusPromise;
      unlisten();
    }
  }
});
</script>

<template>
  <main class="container">
    <LoginForm
      v-if="!isLoggedIn"
      :country="country"
      :email="email"
      :password="password"
      :always-on-top="alwaysOnTop"
      :is-loading="isLoading"
      :is-login-disabled="isLoginDisabled"
      :error-message="errorMessage"
      @update:country="country = $event"
      @update:email="email = $event"
      @update:password="password = $event"
      @update:always-on-top="alwaysOnTop = $event"
      @toggle-always-on-top="toggleAlwaysOnTop"
      @login="handleLogin"
    />

    <CgmDisplay
      v-else
      :cgm-data="cgmData"
      :glucose-background-color="glucoseBackgroundColor"
      @logout="logout"
    />
  </main>
</template>

<style scoped>
.container {
  flex: 1;
  padding: 0;
  display: flex;
  flex-direction: column;
  overflow-y: hidden;
}
</style>
