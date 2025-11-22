# Vue.js Coding Standards - Options API

This project uses Vue 3 with **Options API** and TypeScript. Follow these guidelines when creating new components.

## Component Structure

### Basic Template

```vue
<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "ComponentName",
  
  // Component options follow this order:
  components: {},
  props: {},
  emits: [],
  data() { return {}; },
  computed: {},
  watch: {},
  methods: {},
  
  // Lifecycle hooks
  beforeCreate() {},
  created() {},
  beforeMount() {},
  mounted() {},
  beforeUpdate() {},
  updated() {},
  beforeUnmount() {},
  unmounted() {}
});
</script>

<template>
  <!-- Template content -->
</template>

<style scoped>
/* Scoped styles */
</style>
```

## Props Definition

Always use the object syntax with type validation:

```typescript
props: {
  title: {
    type: String,
    required: true
  },
  count: {
    type: Number,
    default: 0
  },
  items: {
    type: Array as () => string[],
    default: () => []
  },
  user: {
    type: Object as () => User,
    required: false,
    default: null
  }
}
```

## Data Properties

Define with proper TypeScript return type:

```typescript
data() {
  return {
    message: "" as string,
    count: 0 as number,
    isLoading: false as boolean,
    items: [] as string[],
    user: null as User | null
  };
}
```

## Computed Properties

Use proper TypeScript return types:

```typescript
computed: {
  fullName(): string {
    return `${this.firstName} ${this.lastName}`;
  },
  
  filteredItems(): string[] {
    return this.items.filter(item => item.length > 0);
  },
  
  hasItems(): boolean {
    return this.items.length > 0;
  }
}
```

## Methods

### Synchronous Methods

```typescript
methods: {
  handleClick() {
    this.count++;
    this.$emit('update', this.count);
  },
  
  updateMessage(newMessage: string) {
    this.message = newMessage;
  }
}
```

### Async Methods (Tauri Commands)

```typescript
import { invoke } from "@tauri-apps/api/core";

methods: {
  async callRustBackend() {
    try {
      const result = await invoke<string>("greet", { 
        name: this.name 
      });
      this.message = result;
    } catch (error) {
      console.error("Error calling Rust:", error);
      this.message = "Error occurred";
    }
  }
}
```

### Async Methods (C# Backend via TauriDotNetBridge)

```typescript
methods: {
  async callCSharpBackend() {
    try {
      const response = await fetch("http://localhost:5000/api/invoke/MethodName", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          // Request parameters
          name: this.name,
          age: this.age
        }),
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const result = await response.json();
      this.data = result;
    } catch (error) {
      console.error("Error calling C# backend:", error);
      this.errorMessage = "Failed to connect to backend";
    }
  }
}
```

## Emitting Events

Define emits array and use strongly typed emitter:

```typescript
export default defineComponent({
  name: "MyComponent",
  emits: ['update', 'delete', 'save'],
  
  methods: {
    handleSave() {
      this.$emit('save', { id: this.id, data: this.data });
    },
    
    handleDelete() {
      this.$emit('delete', this.id);
    }
  }
});
```

## Lifecycle Hooks Usage

### mounted()
Use for initialization that requires DOM access:

```typescript
mounted() {
  // Initialize third-party libraries
  // Set up event listeners
  // Make initial API calls
  this.loadInitialData();
}
```

### beforeUnmount()
Clean up resources:

```typescript
beforeUnmount() {
  // Remove event listeners
  // Cancel pending requests
  // Clear timers/intervals
  if (this.timer) {
    clearInterval(this.timer);
  }
}
```

## Template Best Practices

### Use v-model for two-way binding
```vue
<input v-model="message" placeholder="Enter message" />
<input v-model.number="age" type="number" />
```

### Event handling
```vue
<button @click="handleClick">Click Me</button>
<form @submit.prevent="handleSubmit">
  <!-- form content -->
</form>
```

### Conditional rendering
```vue
<div v-if="isLoading">Loading...</div>
<div v-else-if="hasError">Error occurred</div>
<div v-else>{{ data }}</div>

<!-- Use v-show for frequent toggles -->
<div v-show="isVisible">Toggle content</div>
```

### List rendering
```vue
<ul>
  <li v-for="item in items" :key="item.id">
    {{ item.name }}
  </li>
</ul>
```

## Import Examples

### Tauri API
```typescript
import { invoke } from "@tauri-apps/api/core";
import { open } from "@tauri-apps/plugin-dialog";
import { appWindow } from "@tauri-apps/api/window";
```

### Child Components
```typescript
import MyButton from "./components/MyButton.vue";

export default defineComponent({
  name: "ParentComponent",
  components: {
    MyButton
  }
});
```

## Error Handling Pattern

```typescript
methods: {
  async fetchData() {
    this.isLoading = true;
    this.error = null;
    
    try {
      const result = await this.callBackend();
      this.data = result;
    } catch (error) {
      console.error("Error fetching data:", error);
      this.error = error instanceof Error ? error.message : "Unknown error";
    } finally {
      this.isLoading = false;
    }
  }
}
```

## Complete Component Example

```vue
<script lang="ts">
import { defineComponent } from "vue";
import { invoke } from "@tauri-apps/api/core";

interface User {
  id: number;
  name: string;
  email: string;
}

export default defineComponent({
  name: "UserProfile",
  
  props: {
    userId: {
      type: Number,
      required: true
    }
  },
  
  emits: ['user-loaded', 'error'],
  
  data() {
    return {
      user: null as User | null,
      isLoading: false as boolean,
      error: null as string | null
    };
  },
  
  computed: {
    userDisplay(): string {
      return this.user ? `${this.user.name} (${this.user.email})` : "";
    },
    
    hasUser(): boolean {
      return this.user !== null;
    }
  },
  
  watch: {
    userId: {
      immediate: true,
      handler(newId: number) {
        this.loadUser(newId);
      }
    }
  },
  
  methods: {
    async loadUser(id: number) {
      this.isLoading = true;
      this.error = null;
      
      try {
        const response = await fetch(`http://localhost:5000/api/invoke/GetUser`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ userId: id })
        });
        
        if (!response.ok) {
          throw new Error("Failed to load user");
        }
        
        this.user = await response.json();
        this.$emit('user-loaded', this.user);
      } catch (error) {
        this.error = error instanceof Error ? error.message : "Unknown error";
        this.$emit('error', this.error);
        console.error("Error loading user:", error);
      } finally {
        this.isLoading = false;
      }
    },
    
    async updateUser(userData: Partial<User>) {
      if (!this.user) return;
      
      const response = await fetch(`http://localhost:5000/api/invoke/UpdateUser`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          userId: this.user.id,
          ...userData
        })
      });
      
      this.user = await response.json();
    }
  },
  
  mounted() {
    console.log("UserProfile component mounted");
  },
  
  beforeUnmount() {
    console.log("UserProfile component unmounting");
  }
});
</script>

<template>
  <div class="user-profile">
    <div v-if="isLoading" class="loading">
      Loading user...
    </div>
    
    <div v-else-if="error" class="error">
      Error: {{ error }}
    </div>
    
    <div v-else-if="hasUser" class="user-content">
      <h2>{{ user!.name }}</h2>
      <p>{{ user!.email }}</p>
      <button @click="loadUser(userId)">Refresh</button>
    </div>
    
    <div v-else class="no-user">
      No user data available
    </div>
  </div>
</template>

<style scoped>
.user-profile {
  padding: 20px;
}

.loading,
.error,
.no-user {
  padding: 10px;
  border-radius: 4px;
}

.error {
  color: red;
  background-color: #fee;
}

.user-content {
  border: 1px solid #ddd;
  padding: 15px;
  border-radius: 8px;
}
</style>
```

## Don't Do This (Composition API)

❌ **Avoid Composition API patterns:**

```vue
<!-- DON'T USE -->
<script setup lang="ts">
import { ref, computed, onMounted } from "vue";

const message = ref("");
const count = ref(0);

const doubled = computed(() => count.value * 2);

onMounted(() => {
  console.log("mounted");
});
</script>
```

✅ **Use Options API instead:**

```vue
<!-- DO USE -->
<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "MyComponent",
  data() {
    return {
      message: "",
      count: 0
    };
  },
  computed: {
    doubled(): number {
      return this.count * 2;
    }
  },
  mounted() {
    console.log("mounted");
  }
});
</script>
```

## Summary

- ✅ Always use `defineComponent` with explicit component name
- ✅ Use TypeScript for type safety
- ✅ Define props with full object syntax
- ✅ Add return types to computed properties and methods
- ✅ Handle errors properly with try-catch
- ✅ Clean up resources in `beforeUnmount()`
- ✅ Use `this` to access component instance properties
- ✅ Emit events using `this.$emit()`
- ❌ Don't use `<script setup>` or Composition API
- ❌ Don't use `ref()`, `reactive()`, `computed()` from vue
