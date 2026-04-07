# Copilot Instructions for Rocket Log PWA

## Project Overview

Rocket Log is a Progressive Web App (PWA) for model rocketry enthusiasts to manage rocket/engine inventory, log launches with weather and GPS data, and track photos. The app is mobile-first, responsive (phone/tablet/desktop), and supports multi-user accounts with admin management of a master catalog.

See `/docs` for the authoritative specification, architecture, and build phases.

## Core Rules

1. **Follow the docs exactly** — `/docs` is the source of truth. Read relevant sections before making decisions:
   - `overview.md` — goals, non-goals, feature scope
   - `architecture.md` — system design, auth strategy, PWA config
   - `tech-stack.md` — required frameworks and libraries
   - `data-model.md` — MongoDB schema
   - `phases.md` — build order (work sequentially)

2. **Tech stack is locked** — Do not introduce new frameworks. Use exactly:
   - **Frontend:** React 18+ with Vite, React Router v6, React Hook Form, React Bootstrap or Mantine, `vite-plugin-pwa`
   - **Backend:** ASP.NET Core Web API (.NET 8), ASP.NET Core Identity (or external OAuth later)
   - **Database:** MongoDB with official `MongoDB.Driver` NuGet package
   - **HTTP:** Axios or native `fetch` wrapper

3. **Use strict TypeScript** on both frontend and backend.

4. **Dev/Demo Auth Bypass**
   - In `appsettings.Development.json`, set `"Auth": { "Enabled": false }`
   - Middleware injects a fixed dev identity, short-circuiting token validation
   - No login UI needed during development; full app is usable

5. **Build Phases**
   - Work through `/docs/phases.md` sequentially
   - Do not jump ahead; each phase builds on the prior one
   - Implement only the current phase

6. **Server-first architecture** — API owns business logic; frontend is presentational.

7. **Ask before deviating** — If you need to deviate from architecture or tech stack, ask first.

## Build, Test, Lint

This is a greenfield project. Commands will depend on the frontend and backend scaffolding:

### Frontend (React + Vite)
```bash
# Install dependencies
npm install

# Dev server (with fast refresh)
npm run dev

# Build for production
npm run build

# Preview production build locally
npm run preview

# Run linter (if ESLint is configured)
npm run lint

# Run tests (if Vitest or Jest is configured)
npm run test
npm run test:watch      # Watch mode for TDD
npm run test -- --run    # Single test run
```

### Backend (ASP.NET Core)
```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run API (debug mode)
dotnet run

# Run specific test project
dotnet test                          # All tests
dotnet test --filter="ClassName"    # Filter by class

# Run linter/analyzer
dotnet build /p:EnforceCodeStyleInBuild=true
```

### Docker (for local dev with MongoDB)
```bash
# Start MongoDB + other services defined in docker-compose.yml
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f [service-name]
```

## High-Level Architecture

### Folder Structure (Frontend)
```
/src
  /api              → API client functions (grouped by resource: users, rockets, launches, etc.)
  /components       → Shared/reusable UI components
  /pages            → Top-level route components
    /inventory      → My Rockets, My Engines, Accessories
    /launches       → Launch log list, detail, new launch form
    /catalog        → Master rockets/engines browser
    /settings       → User settings
  /hooks            → Custom hooks (useGeolocation, useWeather, useAuth, etc.)
  /context          → Global state (AuthContext, AppState, etc.)
  /utils            → Helpers, formatters, validators
  /assets           → Static images, icons, theme files
```

### Folder Structure (Backend)
```
/src
  /Controllers      → Route handlers (UsersController, RocketsController, LaunchesController, etc.)
  /Services         → Business logic (UserService, LaunchService, WeatherService, etc.)
  /Models           → Request/response DTOs
  /Data             → MongoDB context, repositories
  /Middleware       → Auth bypass middleware, error handling
  /Seeders          → DataSeeder for master catalog bootstrap
```

### Key Data Flows

1. **Authentication**
   - In dev/demo: Middleware injects fixed identity (bypass enabled)
   - In production: ASP.NET Core Identity or OAuth/OIDC issuing JWTs
   - Frontend stores JWT in localStorage; includes in `Authorization: Bearer` header
   - API validates JWT in middleware; populates `HttpContext.User`

2. **New Launch Form Flow**
   - User taps "New Launch"
   - Browser's Geolocation API captures lat/lng
   - Frontend calls `GET /api/v1/weather?lat=...&lng=...`
   - Backend fetches from OpenWeatherMap, reverse-geocodes location name
   - Backend returns weather + location; frontend renders in form
   - User can override weather fields manually
   - On submit, POST to `/api/v1/launches` with location + weather snapshot

3. **Master Catalog**
   - Developer seeds MongoDB with rockets/engines JSON (via DataSeeder)
   - Frontend fetches from `GET /api/v1/rockets` and `GET /api/v1/engines` (cached by PWA)
   - Users add items to personal inventory (user_rockets, user_engines collections)
   - Admin panel (role-gated) allows CRUD on master catalog items

4. **Image Storage**
   - User photos (build photos, launch photos): Upload to `/api/v1/upload`, stored on filesystem (dev) or cloud blob (prod)
   - API returns public URL; stored in user_rockets, launches documents
   - Master catalog images (box art, thumbnails): Developer uploads via admin panel

### PWA Configuration
- Service worker generated by `vite-plugin-pwa` + Workbox (GenerateSW mode)
- **Static assets (CSS, JS, images):** Cache-first
- **Master catalog GET:** Stale-while-revalidate (data changes rarely)
- **User data GET:** Network-first with cache fallback (readable offline)
- **Offline writes:** Not supported in v1 — "New Launch" form shows warning and disables submit when offline

## Key Conventions

1. **API Endpoints**
   - Versioned: `/api/v1/...`
   - Resource-based: `/api/v1/rockets`, `/api/v1/launches`, etc.
   - Paginated list responses include: `items`, `total`, `page`, `pageSize`

2. **Error Handling**
   - Backend returns standard HTTP status codes + JSON error body: `{ "error": "message", "code": "ErrorCode" }`
   - Frontend wraps API calls in try/catch; displays user-friendly messages

3. **TypeScript**
   - Frontend: Interfaces for API DTOs, component props, hook returns
   - Backend: C# classes for models; return typed responses; avoid `dynamic`

4. **State Management**
   - Frontend: React Context + `useReducer` for global auth/app state
   - Local component state: Use `useState` and `useReducer` as needed
   - If state grows complex, migrate to Zustand (lightweight, approachable)

5. **Form Validation**
   - React Hook Form with built-in validation rules
   - Server-side validation always; never trust client-only validation

6. **Environment Config**
   - Frontend: `.env` files (Vite auto-loads `.env`, `.env.local`, `.env.[mode]`)
   - Backend: `appsettings.json` + `appsettings.Development.json` + secrets (dev) or Key Vault (prod)

7. **Commit Messages**
   - Format: `[Phase N] Brief description of change`
   - Example: `[Phase 2] Add rocket catalog search`
   - Reference issues/PRs when relevant: `Fixes #123`

## When Generating Code

1. Read the relevant `/docs` sections first
2. Verify which phase you're in (see `/docs/phases.md`)
3. Check the data model (see `/docs/data-model.md`)
4. Implement only the current phase; do not add features from later phases
5. Create an implementation plan for multi-step work (multiple files, new concepts)
6. Ask before deviating from architecture

## Useful Links

- **Docs:** `/docs` folder (overview, architecture, tech-stack, data-model, api-spec, phases)
- **COPILOT.md:** `/COPILOT.md` — Quick reference for Copilot instructions (also read as a reminder)
- **.squad:** `.squad` folder — Squad workflow configuration for multi-agent development
