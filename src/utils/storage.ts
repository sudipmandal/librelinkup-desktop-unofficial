import { invoke } from '@tauri-apps/api/core';
import { Store } from '@tauri-apps/plugin-store';

// Initialize the store for non-sensitive data
const store = await Store.load('app-settings.json');

/**
 * Storage utility for cross-platform data persistence
 * 
 * Two storage mechanisms:
 * 1. Store Plugin - For non-sensitive data (preferences, UI state)
 * 2. Secure Storage - For sensitive data (tokens, passwords) using OS keyring
 */

// ===== NON-SENSITIVE DATA STORAGE (Store Plugin) =====

/**
 * Save a non-sensitive value to the store
 * @param key - Storage key
 * @param value - Value to store (will be JSON serialized)
 */
export async function savePreference(key: string, value: any): Promise<void> {
  await store.set(key, value);
  await store.save(); // Persist to disk
}

/**
 * Get a non-sensitive value from the store
 * @param key - Storage key
 * @param defaultValue - Default value if key doesn't exist
 * @returns Stored value or default
 */
export async function getPreference<T>(key: string, defaultValue?: T): Promise<T | null> {
  const value = await store.get<T>(key);
  return value !== null && value !== undefined ? value : (defaultValue ?? null);
}

/**
 * Delete a non-sensitive value from the store
 * @param key - Storage key
 */
export async function deletePreference(key: string): Promise<void> {
  await store.delete(key);
  await store.save();
}

/**
 * Get all keys in the store
 * @returns Array of all keys
 */
export async function getAllPreferenceKeys(): Promise<string[]> {
  return await store.keys();
}

/**
 * Clear all non-sensitive data
 */
export async function clearAllPreferences(): Promise<void> {
  await store.clear();
  await store.save();
}

// ===== SECURE DATA STORAGE (OS Keyring/Keychain) =====

/**
 * Store a secure value (token, password, etc.) in OS credential manager
 * @param service - Service name (e.g., 'tauri-app')
 * @param key - Key identifier (e.g., 'api-token')
 * @param value - Secure value to store
 */
export async function storeSecure(service: string, key: string, value: string): Promise<void> {
  try {
    await invoke('store_secure', { service, key, value });
  } catch (error) {
    throw new Error(`Failed to store secure value: ${error}`);
  }
}

/**
 * Retrieve a secure value from OS credential manager
 * @param service - Service name
 * @param key - Key identifier
 * @returns Stored secure value
 */
export async function getSecure(service: string, key: string): Promise<string> {
  try {
    return await invoke<string>('get_secure', { service, key });
  } catch (error) {
    throw new Error(`Failed to retrieve secure value: ${error}`);
  }
}

/**
 * Delete a secure value from OS credential manager
 * @param service - Service name
 * @param key - Key identifier
 */
export async function deleteSecure(service: string, key: string): Promise<void> {
  try {
    await invoke('delete_secure', { service, key });
  } catch (error) {
    throw new Error(`Failed to delete secure value: ${error}`);
  }
}

// ===== CONVENIENCE METHODS =====

const APP_SERVICE_NAME = 'tauri-app';

/**
 * Store an API token securely
 * @param token - API token value
 */
export async function storeApiToken(token: string): Promise<void> {
  await storeSecure(APP_SERVICE_NAME, 'api-token', token);
}

/**
 * Retrieve the stored API token
 * @returns API token or null if not found
 */
export async function getApiToken(): Promise<string | null> {
  try {
    return await getSecure(APP_SERVICE_NAME, 'api-token');
  } catch (error) {
    return null; // Token not found
  }
}

/**
 * Delete the stored API token
 */
export async function deleteApiToken(): Promise<void> {
  await deleteSecure(APP_SERVICE_NAME, 'api-token');
}

/**
 * Store user credentials securely
 * @param username - Username
 * @param password - Password
 */
export async function storeCredentials(username: string, password: string): Promise<void> {
  await storeSecure(APP_SERVICE_NAME, 'username', username);
  await storeSecure(APP_SERVICE_NAME, 'password', password);
}

/**
 * Retrieve stored user credentials
 * @returns Object with username and password, or null if not found
 */
export async function getCredentials(): Promise<{ username: string; password: string } | null> {
  try {
    const username = await getSecure(APP_SERVICE_NAME, 'username');
    const password = await getSecure(APP_SERVICE_NAME, 'password');
    return { username, password };
  } catch (error) {
    return null;
  }
}

/**
 * Delete stored credentials
 */
export async function deleteCredentials(): Promise<void> {
  try {
    await deleteSecure(APP_SERVICE_NAME, 'username');
  } catch {}
  try {
    await deleteSecure(APP_SERVICE_NAME, 'password');
  } catch {}
}

// ===== USAGE EXAMPLES =====

/**
 * Example: Store user preferences
 * 
 * await savePreference('theme', 'dark');
 * await savePreference('language', 'en');
 * await savePreference('windowPosition', { x: 100, y: 200 });
 * 
 * const theme = await getPreference('theme', 'light');
 */

/**
 * Example: Store sensitive data
 * 
 * await storeApiToken('your-secret-token-here');
 * const token = await getApiToken();
 * 
 * await storeCredentials('user@example.com', 'password123');
 * const creds = await getCredentials();
 */
