<script lang="ts">
import { defineComponent } from "vue";
import { getCurrentWindow } from "@tauri-apps/api/window";

export default defineComponent({
  name: "TitleBar",
  props: {
    isLinux: {
      type: Boolean,
      required: true
    },
    showToolbar: {
      type: Boolean,
      required: true
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
  }
});
</script>

<template>
  <div v-if="isLinux" class="titlebar linux-titlebar" :class="{ 'titlebar-visible': showToolbar }" data-tauri-drag-region>
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
  </div>
</template>

<style scoped>
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

.linux-titlebar {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  z-index: 999;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s ease;
  background: rgba(255, 255, 255, 0.95);
}

.linux-titlebar.titlebar-visible {
  opacity: 1;
  pointer-events: auto;
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

@media (prefers-color-scheme: dark) {
  .titlebar {
    background: linear-gradient(180deg, #3a3a3a 0%, #2f2f2f 100%);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }

  .linux-titlebar {
    background: rgba(58, 58, 58, 0.95);
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
}
</style>
