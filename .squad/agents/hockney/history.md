# Hockney — History & Learnings

**Created:** 2026-04-04  
**Project:** Rocket Log PWA  
**User:** Brady  

## Project Context

**Frontend Stack:**
- React 18 + Vite (dev server, fast HMR, production build)
- React Router v6 (routing)
- React Hook Form (form validation, minimal re-renders)
- React Bootstrap OR Mantine (UI components)
- vite-plugin-pwa (PWA setup, manifest, service worker)
- Axios or fetch wrapper (HTTP client)

**Folder Structure (Phase 0 scaffold):**
```
/frontend
  /src
    /api              (API client functions)
    /components       (reusable UI components)
    /pages            (route-level components: Login, Inventory, Launches, etc.)
    /hooks            (custom hooks: useAuth, useGeolocation, useWeather, etc.)
    /context          (React Context for global state: AuthContext, AppState)
    /utils            (helpers, formatters, validators)
    /assets           (static images, icons, theme)
  package.json
  vite.config.ts
  tsconfig.json
  index.html
```

**Key Constraints:**
- React 18+ only
- Strict TypeScript
- Mobile-first responsive design
- PWA service worker for offline-friendly caching
- Dev auth bypass: no login UI needed during development

**Team Coordination:**
- Fenster owns the API endpoints. Hockney consumes them via contract negotiation.
- Kobayashi writes integration tests and form validation tests.
- Keaton approves UI library choice and folder structure.

## Current Status

**Phase:** 0 (Project Setup)  
**Progress:** Team hired, squad infrastructure created. Awaiting start signal for Phase 0 project scaffolding.

## Decisions Affecting Frontend

- UI library choice (React Bootstrap or Mantine): TBD by Hockney, approval by Keaton
- Caching strategy: Cache-first statics, stale-while-revalidate catalog, network-first user data
- Auth flow: JWT token in localStorage, `Authorization: Bearer` header on all API calls
- Image uploads: Multipart form POST to `/api/v1/upload`, API returns public URL

## Learnings

(To be populated during work)
