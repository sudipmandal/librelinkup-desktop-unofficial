<script lang="ts">
import { defineComponent } from "vue";
import TitleBar from "./components/TitleBar.vue";
import AppSession from "./components/AppSession.vue";

export default defineComponent({
  name: "App",
  components: {
    TitleBar,
    AppSession
  },
  data() {
    return {
      isLinux: false,
      showToolbar: false,
    };
  },
  async mounted() {
    this.isLinux = navigator.platform.toLowerCase().includes("linux");
    console.log(`Platform: ${navigator.platform}, isLinux: ${this.isLinux}`);
  }
});
</script>

<template>
  <div class="app-window" :class="{ 'app-drag-region': !isLinux }"
       :data-tauri-drag-region="!isLinux ? '' : undefined"
       @mouseenter="showToolbar = isLinux"
       @mouseleave="showToolbar = false">
    <TitleBar
      :is-linux="isLinux"
      :show-toolbar="showToolbar"
    />

    <AppSession />
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
}

.app-drag-region {
  -webkit-app-region: drag;
}

@media (prefers-color-scheme: dark) {
  :root {
    color: #f6f6f6;
  }

  .app-window {
    background: #2f2f2f;
  }
}
</style>
