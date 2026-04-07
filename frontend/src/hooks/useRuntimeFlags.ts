export function useRuntimeFlags() {
  return {
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080',
    authEnabled: import.meta.env.VITE_ENABLE_AUTH === 'true',
  }
}