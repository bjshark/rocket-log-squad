# Kobayashi — History & Learnings

**Created:** 2026-04-04  
**Project:** Rocket Log PWA  
**User:** Brady  

## Project Context

**Test Strategy:**
- Frontend: Jest or Vitest (fast, React-native, snapshot testing)
- Backend: xUnit or NUnit (.NET native, approachable)
- Coverage target: 70%+ on business logic, 50%+ overall
- Test-first for bug reproduction: Always write test that fails first

**Test Categories by Phase:**

**Phase 0 (Setup):**
- Sample unit tests for utilities and services
- Test framework configuration
- Folder structure established

**Phase 1 (Auth):**
- Auth context tests (token storage, expiration, refresh)
- JWT middleware tests (dev bypass, token validation)
- Protected route tests

**Phase 2 (Master Catalog):**
- API endpoint tests (GET /api/v1/rockets, /api/v1/engines)
- Seed data validation tests
- Caching behavior tests (stale-while-revalidate)

**Phase 3 (User Inventory):**
- Inventory CRUD tests
- Form validation tests (add from catalog, nickname, upload)
- Photo upload flow tests

**Phase 4 (Launch Logging):**
- New Launch form tests (geolocation, weather fetching, offline warning)
- Launch list and detail view tests
- Edit/delete launch tests

**Phase 5 (PWA):**
- Service worker cache tests
- Offline detection and UI behavior
- Install-to-home-screen tests (manual on device)

**Phase 6 (Admin):**
- Role-gated admin panel tests
- Master catalog CRUD tests
- Seed export tests

**Key Test Files:**
- Frontend: `__tests__/` folders co-located with code or `src/__tests__/` centralized
- Backend: `Tests/` folder parallel to `src/`

## Current Status

**Phase:** 0 (Project Setup)  
**Progress:** Team hired, squad infrastructure created. Awaiting start signal for Phase 0 project scaffolding.

## Decisions Affecting QA

- Test framework choice (Jest/Vitest for frontend, xUnit/NUnit for backend): TBD by Kobayashi, approval by Keaton
- Coverage target: 70% on business logic, 50% overall
- Bug reproduction workflow: Test first, then fix

## Learnings

(To be populated during work)
