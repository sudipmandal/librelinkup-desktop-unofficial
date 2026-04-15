import { createApp } from "vue";
import App from "./App.vue";
import { installLogger } from "./lib/logger";

installLogger();
createApp(App).mount("#app");
