<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "CgmDisplay",
  props: {
    cgmData: {
      type: Object,
      required: true
    },
    glucoseBackgroundColor: {
      type: String,
      required: true
    }
  },
  emits: ["logout"],
  methods: {
    getTrendArrow(trend: number): string {
      const arrows: Record<number, string> = {
        1: '↓↓',
        2: '↓',
        3: '→',
        4: '↑',
        5: '↑↑',
      };
      return arrows[trend] || '?';
    },
    formatTimestamp(timestamp: string): string {
      const date = new Date(timestamp);
      return date.toLocaleString();
    }
  }
});
</script>

<template>
  <div class="cgm-container" :style="{ background: glucoseBackgroundColor }">
    <button @click="$emit('logout')" class="btn-logout-gear" title="Logout">
      <span>⚙</span>
    </button>
    
    <div v-if="cgmData?.glucoseMeasurement" class="glucose-display" :style="{ background: glucoseBackgroundColor }">
      <div class="glucose-time">
        {{ formatTimestamp(cgmData.glucoseMeasurement.Timestamp) }}
      </div>
      <div class="glucose-value">
        {{ cgmData.glucoseMeasurement.Value }}
        <span class="glucose-unit">{{ cgmData.glucoseMeasurement.ValueInMgPerDl ? 'mmol/L' : 'mg/dL' }}</span>
        <span class="glucose-trend" v-if="cgmData.glucoseMeasurement.TrendArrow">{{ getTrendArrow(cgmData.glucoseMeasurement.TrendArrow) }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cgm-container {
  padding: 20px;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  height: 100%;
  flex: 1;
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
  padding: 0;
  margin-bottom: 0px;
  transition: background 0.3s ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
}

.glucose-value {
  font-size: 24px;
  font-weight: bold;
  line-height: 1;
  display: flex;
  align-items: center;
  gap: 10px;
}

.glucose-unit {
  font-size: 12px;
  font-weight: normal;
  opacity: 0.9;
  display: inline-flex;
  align-items: center;
}

.glucose-trend {
  font-size: 24px;
  display: inline-flex;
  align-items: center;
}

.glucose-time {
  font-size: 9px;
  opacity: 0.7;
  position: absolute;
  top: 0px;
  left: 10px;
}

@media (prefers-color-scheme: dark) {
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
}
</style>
