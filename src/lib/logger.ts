import { reactive } from 'vue';

const MAX_LINES = 200;

export const logStore = reactive({
  lines: [] as string[],
});

function formatArg(arg: unknown): string {
  if (typeof arg === 'string') return arg;
  try {
    return JSON.stringify(arg);
  } catch {
    return String(arg);
  }
}

function pushLog(level: string, args: unknown[]) {
  const ts = new Date().toLocaleTimeString();
  const msg = args.map(formatArg).join(' ');
  logStore.lines.push(`[${ts}] ${level} ${msg}`);
  if (logStore.lines.length > MAX_LINES) {
    logStore.lines.splice(0, logStore.lines.length - MAX_LINES);
  }
}

const originalLog = console.log.bind(console);
const originalError = console.error.bind(console);
const originalWarn = console.warn.bind(console);

export function installLogger() {
  console.log = (...args: unknown[]) => {
    originalLog(...args);
    pushLog('', args);
  };
  console.error = (...args: unknown[]) => {
    originalError(...args);
    pushLog('[ERR]', args);
  };
  console.warn = (...args: unknown[]) => {
    originalWarn(...args);
    pushLog('[WARN]', args);
  };
}
