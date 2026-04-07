# Keaton — History & Learnings

**Created:** 2026-04-04  
**Project:** Rocket Log PWA  
**User:** Brady  

## Project Context

**Domain:** Model Rocketry PWA for inventory and launch logging.

**Stack:**
- Frontend: React 18 + Vite + React Bootstrap/Mantine
- Backend: ASP.NET Core .NET 8 + MongoDB
- Auth: JWT with dev bypass (middleware injects identity when `Auth.Enabled: false`)

**Scope:**
- Inventory (rockets, engines, accessories)
- Launch logging with weather/GPS snapshot
- Master catalog (developer-managed seed)
- Multi-user support
- Mobile-first, responsive PWA

**Phases (Sequential):**
0. Project Setup
1. Auth & User Shell
2. Master Catalog
3. User Inventory
4. Launch Logging
5. PWA Polish & Offline
6. Admin & Data Management

**Team:**
- Keaton (you): Lead
- Hockney: Frontend Dev (React)
- Fenster: Backend Dev (ASP.NET Core)
- Kobayashi: Tester
- Scribe: Session logger (silent)
- Ralph: Work monitor

## Current Status

**Phase:** 0 (Project Setup)  
**Progress:** Team hired, squad infrastructure created. Ready to begin project scaffolding.

## Decisions Recorded

All 10 foundational decisions are recorded in `.squad/decisions.md`. Key anchors:
- Tech stack locked (no experiments)
- Dev auth bypass enabled
- Monorepo structure (frontend/ + backend/)
- Phase sequencing enforced
- Image storage: filesystem (dev), can upgrade to cloud (prod)
- Caching: cache-first for statics, stale-while-revalidate for catalog, network-first for user data

## Learnings

(To be populated during work)
