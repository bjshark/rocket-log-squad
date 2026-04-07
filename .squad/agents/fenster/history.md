# Fenster — History & Learnings

**Created:** 2026-04-04  
**Project:** Rocket Log PWA  
**User:** Brady  

## Project Context

**Backend Stack:**
- ASP.NET Core .NET 8 Web API
- MongoDB (via official MongoDB.Driver NuGet package)
- JWT bearer token auth (ASP.NET Core Identity or custom)
- Dev auth bypass: `appsettings.Development.json` sets `Auth.Enabled: false`
- OpenWeatherMap API for weather snapshots
- Image storage: filesystem (dev), cloud blob (prod) TBD

**Folder Structure (Phase 0 scaffold):**
```
/backend
  /src
    /Controllers      (route handlers: UsersController, RocketsController, LaunchesController)
    /Services         (business logic: UserService, LaunchService, WeatherService)
    /Models           (DTOs and request/response models)
    /Data             (MongoDB context, repositories)
    /Middleware       (auth bypass, error handling)
    /Seeders          (DataSeeder for master catalog)
  appsettings.json
  appsettings.Development.json
  Startup.cs or Program.cs
  docker-compose.yml
```

**Data Models (MongoDB Collections, from `/docs/data-model.md`):**
- `users` — username, email, roles, created_at
- `user_rockets` — user_id, rocket_id (catalog ref), nickname, build_date, condition, build_photo_url
- `user_engines` — user_id, engine_id (catalog ref), quantity_on_hand
- `user_accessories` — user_id, name, quantity, description
- `launches` — user_id, rocket_id, engine_id, date, location_lat/lng, location_name, weather snapshot, notes, outcome, photo_url
- `rockets` (master catalog) — model_name, manufacturer, specs, image_url, created_at
- `engines` (master catalog) — model_name, manufacturer, impulse_class, motor_type, specs, created_at

**API Versioning:** All endpoints prefixed `/api/v1/`

**Auth Flow:**
- Dev: `Auth.Enabled: false` → middleware injects fixed identity → full app usable without token
- Prod: `Auth.Enabled: true` → JWT validation required → token issued by Identity or external OAuth

**Phase 0 Goal:**
- Scaffolding complete
- MongoDB connected locally (Docker)
- Auth middleware working (dev bypass tested)
- Seed data loaded on startup

## Current Status

**Phase:** 0 (Project Setup)  
**Progress:** Team hired, squad infrastructure created. Awaiting start signal for Phase 0 project scaffolding.

## Decisions Affecting Backend

- Monorepo: backend and frontend in same repo, docker-compose at root
- Image storage: filesystem (dev), upgrade to cloud later (prod)
- Auth: JWT with dev bypass via `appsettings.Development.json` flag
- Seed data: JSON file in `/backend/seeds/`, loaded by `DataSeeder` on app startup

## Learnings

(To be populated during work)
