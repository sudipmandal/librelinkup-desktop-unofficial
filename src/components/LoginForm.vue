<script lang="ts">
import { defineComponent } from "vue";
import { logStore } from "../lib/logger";

export default defineComponent({
  name: "LoginForm",
  props: {
    country: {
      type: String,
      required: true
    },
    email: {
      type: String,
      required: true
    },
    password: {
      type: String,
      required: true
    },
    alwaysOnTop: {
      type: Boolean,
      required: true
    },
    isLoading: {
      type: Boolean,
      required: true
    },
    isLoginDisabled: {
      type: Boolean,
      required: true
    },
    errorMessage: {
      type: String,
      default: ""
    }
  },
  emits: [
    "update:country",
    "update:email",
    "update:password",
    "update:alwaysOnTop",
    "login",
    "toggle-always-on-top"
  ],
  data() {
    return {
      logStore
    };
  },
  watch: {
    'logStore.lines': {
      handler() {
        this.$nextTick(() => {
          const el = this.$refs.debugLogArea as HTMLTextAreaElement;
          if (el) el.scrollTop = el.scrollHeight;
        });
      },
      deep: true
    }
  },
  methods: {
    onAlwaysOnTopChange(event: Event) {
      const checked = (event.target as HTMLInputElement).checked;
      this.$emit("update:alwaysOnTop", checked);
      this.$emit("toggle-always-on-top");
    }
  }
});
</script>

<template>
  <div>
    <form class="login-form" @submit.prevent="$emit('login')">
      <div class="form-row">
        <div class="form-group">
          <label for="country">Country</label>
          <select id="country" :value="country" @change="$emit('update:country', ($event.target as HTMLSelectElement).value)" required>
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
              :checked="alwaysOnTop" 
              @change="onAlwaysOnTopChange"
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
            :value="email"
            @input="$emit('update:email', ($event.target as HTMLInputElement).value)"
            placeholder="Enter your email"
            required 
          />
        </div>
        
        <div class="form-group">
          <label for="password">Password</label>
          <input 
            id="password" 
            type="password" 
            :value="password"
            @input="$emit('update:password', ($event.target as HTMLInputElement).value)"
            placeholder="Enter your password"
            required 
          />
        </div>
      </div>
    </form>
    
    <div class="button-container">
      <button @click="$emit('login')" class="btn-login" :disabled="isLoginDisabled || isLoading">
        {{ isLoading ? 'Logging in...' : 'Login' }}
      </button>
    </div>
    
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>
    
    <div v-if="logStore.lines.length" class="debug-log-container">
      <label>Log</label>
      <textarea class="debug-log" readonly :value="logStore.lines.join('\n')" ref="debugLogArea"></textarea>
    </div>
  </div>
</template>

<style scoped>
.login-form {
  max-width: 800px;
  margin: 0 auto;
  padding: 30px;
  background: white;
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

.error-message {
  text-align: center;
  margin-top: 20px;
  padding: 12px 20px;
  background: #fff3cd;
  border: 1px solid #ffc107;
  border-radius: 8px;
  color: #856404;
  font-size: 14px;
  font-weight: 500;
  max-width: 800px;
  margin-left: auto;
  margin-right: auto;
  box-shadow: 0 2px 4px rgba(255, 193, 7, 0.2);
}

.debug-log-container {
  max-width: 800px;
  margin: 15px auto 0;
}

.debug-log-container label {
  font-size: 12px;
  font-weight: 500;
  color: #666;
  display: block;
  margin-bottom: 4px;
}

.debug-log {
  width: 100%;
  height: 120px;
  font-family: monospace;
  font-size: 11px;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: #1e1e1e;
  color: #d4d4d4;
  resize: vertical;
  white-space: pre;
  overflow-y: auto;
}

@media (prefers-color-scheme: dark) {
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

  .error-message {
    background: #3a2f1f;
    border-color: #ffc107;
    color: #ffd966;
  }

  .debug-log-container label {
    color: #aaa;
  }

  .debug-log {
    border-color: #444;
  }
}
</style>
