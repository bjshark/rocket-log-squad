import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      devOptions: {
        enabled: true,
      },
      includeAssets: ['favicon.svg', 'pwa-192.svg', 'pwa-512.svg', 'splash-1170x2532.svg', 'splash-2532x1170.svg'],
      manifestFilename: 'manifest.webmanifest',
      workbox: {
        cleanupOutdatedCaches: true,
        navigateFallback: '/index.html',
        navigateFallbackDenylist: [/^\/api\//],
        globPatterns: ['**/*.{js,css,html,svg,png,ico}'],
        runtimeCaching: [
          {
            urlPattern: ({ request }) =>
              request.destination === 'document' ||
              request.destination === 'script' ||
              request.destination === 'style' ||
              request.destination === 'image' ||
              request.destination === 'font',
            handler: 'CacheFirst',
            options: {
              cacheName: 'rocket-log-shell',
              expiration: {
                maxEntries: 120,
                maxAgeSeconds: 60 * 60 * 24 * 30,
              },
            },
          },
          {
            urlPattern: ({ url, request }) =>
              request.method === 'GET' && /\/api\/v1\/(rockets|engines)(\/|$)/.test(url.pathname),
            handler: 'StaleWhileRevalidate',
            options: {
              cacheName: 'rocket-log-catalog',
              cacheableResponse: {
                statuses: [0, 200],
              },
              expiration: {
                maxEntries: 60,
                maxAgeSeconds: 60 * 60 * 24,
              },
            },
          },
          {
            urlPattern: ({ url, request }) =>
              request.method === 'GET' && /\/api\/v1\/(my|weather)(\/|$)/.test(url.pathname),
            handler: 'NetworkFirst',
            options: {
              cacheName: 'rocket-log-user-data',
              networkTimeoutSeconds: 3,
              cacheableResponse: {
                statuses: [0, 200],
              },
              expiration: {
                maxEntries: 80,
                maxAgeSeconds: 60 * 60 * 6,
              },
            },
          },
        ],
      },
      manifest: {
        id: '/',
        name: 'Rocket Log',
        short_name: 'Rocket Log',
        description: 'Mobile-first rocket inventory and launch logging PWA.',
        theme_color: '#f36b21',
        background_color: '#fff8ef',
        orientation: 'portrait-primary',
        display: 'standalone',
        display_override: ['standalone', 'browser'],
        prefer_related_applications: false,
        start_url: '/',
        icons: [
          {
            src: '/pwa-192.svg',
            sizes: '192x192',
            type: 'image/svg+xml',
            purpose: 'any',
          },
          {
            src: '/pwa-512.svg',
            sizes: '512x512',
            type: 'image/svg+xml',
            purpose: 'any maskable',
          },
        ],
        screenshots: [
          {
            src: '/splash-1170x2532.svg',
            sizes: '1170x2532',
            type: 'image/svg+xml',
            form_factor: 'narrow',
            label: 'Rocket Log mobile launch logging view',
          },
          {
            src: '/splash-2532x1170.svg',
            sizes: '2532x1170',
            type: 'image/svg+xml',
            form_factor: 'wide',
            label: 'Rocket Log wide layout with launch log and inventory',
          },
        ],
      },
    }),
  ],
})