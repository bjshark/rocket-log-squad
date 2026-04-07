# Rocket Log Squad - Decisions Ledger

## Active Decisions

### D-001: Phase Sequence Is Locked
- Date: 2026-04-04
- Decision: Work starts at Phase 0 and proceeds exactly in the order defined in docs/phases.md.
- Reason: Later phases depend on project scaffolding, auth shape, and catalog foundations.

### D-002: Monorepo Structure
- Date: 2026-04-04
- Decision: Use a single repository with frontend/ and backend/ at the root.
- Reason: Simpler local development, one docker-compose file, shared docs and CI.

### D-003: Frontend Stack
- Date: 2026-04-04
- Decision: Use React 18 + Vite + React Router v6 + React Hook Form + vite-plugin-pwa.
- Reason: This is the documented stack and matches the PWA/mobile-first requirement.

### D-004: Backend Stack
- Date: 2026-04-04
- Decision: Use ASP.NET Core Web API on .NET 8 with MongoDB.Driver.
- Reason: This is the documented stack and keeps backend implementation aligned with the spec.

### D-005: Development Auth Bypass
- Date: 2026-04-04
- Decision: In development, appsettings.Development.json will set Auth.Enabled to false and middleware will inject a fixed dev identity.
- Reason: The docs explicitly require a full dev/demo bypass so Phase 1 can move quickly.

### D-006: Local Development Runtime
- Date: 2026-04-04
- Decision: Use Docker Compose for MongoDB and run frontend/backend directly during development.
- Reason: Keeps MongoDB reproducible while preserving fast frontend and backend inner loops.

### D-007: Phase 1 Auth Approach
- Date: 2026-04-04
- Decision: Implement frontend auth context and protected routes using the development bypass session from `/api/v1/users/me` while keeping login/register as explicit stubs.
- Reason: Matches Phase 1 scope and avoids jumping ahead into full production auth flows.

### D-008: Phase 1 Navigation Shell
- Date: 2026-04-04
- Decision: Use desktop top navigation plus a mobile bottom navigation bar for primary app routes.
- Reason: Satisfies the mobile-first navigation shell requirement for Phase 1.

### D-009: Docker Backend Connection Override
- Date: 2026-04-04
- Decision: Set `ConnectionStrings__MongoDb` in `docker-compose.yml` for backend container runtime.
- Reason: Prevents backend startup failure caused by fallback to container-local `localhost:27017`.

### D-010: Master Catalog Seed Files
- Date: 2026-04-04
- Decision: Store master catalog data in versioned JSON files at `backend/Data/Seeds/rockets.json` and `backend/Data/Seeds/engines.json`.
- Reason: Matches documented seed strategy and keeps catalog updates reviewable through source control.

### D-011: Catalog Seeder Upsert Strategy
- Date: 2026-04-04
- Decision: Seeder performs idempotent bulk upserts keyed by `(manufacturer, sku)` for rockets and `(manufacturer, designation)` for engines.
- Reason: Supports safe repeat startup seeding without duplicate master records.

### D-012: Catalog Indexing
- Date: 2026-04-04
- Decision: Create unique indexes for catalog identities plus secondary search indexes on commonly-filtered fields.
- Reason: Preserves data integrity and improves future catalog query performance.

## Governance

- All meaningful changes require team consensus.
- Document architectural decisions here.
- Keep history focused on work, decisions focused on direction.

## Current Focus

- Phase 2 - Master Catalog
- Seed data source and idempotent seeding complete
- Next: catalog read endpoints and browse/search UI
