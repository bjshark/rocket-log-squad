# Release-Readiness Checklist

Use this checklist for a final go/no-go decision before handing off or deploying.

## 1) Build And Test Gates

Mark each item complete before proceeding.

- [ ] Backend solution builds successfully
- [ ] Frontend production build succeeds
- [ ] Integration tests pass (Mongo2Go)
- [ ] Backend health endpoint returns status ok

Run from repository root:

```bash
# Backend + solution
 dotnet restore rocket-log.sln
 dotnet build rocket-log.sln

# Frontend
 npm --prefix frontend install
 npm --prefix frontend run build

# Integration tests (Mongo2Go)
 dotnet test tests/RocketLog.Api.IntegrationTests/RocketLog.Api.IntegrationTests.csproj
```

Health check:

```bash
# Adjust port if running backend on a non-default local port
curl -s http://localhost:8080/api/v1/system/health
```

Expected shape includes:
- status = ok
- environment present
- authEnabled present

## 2) Local Runtime Gates

- [ ] Backend starts cleanly in Development
- [ ] Frontend dev server starts cleanly
- [ ] Frontend can load app shell and call API without CORS errors
- [ ] CORS allowed origins match the active frontend origin/port

Recommended local startup:

```bash
# Backend
 dotnet run --project backend/RocketLog.Api.csproj

# Frontend
 npm --prefix frontend run dev
```

If using Docker backend:

```bash
docker compose up -d --build backend
```

If CORS issues appear, verify:
- backend/appsettings.Development.json includes the exact frontend origin in Cors:AllowedOrigins
- VITE_API_BASE_URL points at the active backend URL

## 3) Product Smoke Checks

### Core User Flows

- [ ] Authentication shell loads and user context resolves in dev mode
- [ ] Catalog browse/search/filter works
- [ ] Add to inventory from catalog works
- [ ] Inventory pages support list/update/delete workflows
- [ ] Launch create/list/detail/update/delete works
- [ ] Launch photo upload works
- [ ] Offline warning appears on New Launch and submit is blocked while offline

### Admin Flows

- [ ] Admin route is visible only to admin role
- [ ] Admin master rocket CRUD works
- [ ] Admin master engine CRUD works
- [ ] Admin image upload works
- [ ] Admin seed export works

## 4) PWA Readiness Checks

- [ ] Build emits manifest.webmanifest
- [ ] Build emits sw.js and workbox runtime file
- [ ] Manifest includes icons and screenshots
- [ ] iOS web app metadata exists in index.html

Manual browser checks:

- [ ] Desktop install prompt/install flow is functional
- [ ] Service worker is registered and active
- [ ] Offline shell behavior works as expected for cached routes

Manual device checks:

- [ ] Android Add to Home Screen launch works
- [ ] iOS Add to Home Screen launch works

## 5) Documentation And Configuration Checks

- [ ] docs/phases.md reflects completed phases through Phase 6
- [ ] docs/misc.md local run instructions are accurate
- [ ] Environment variables are documented and current
- [ ] No temporary/debug TODO markers remain in shipped docs

## 6) Known Warnings Review

These do not block release by default, but should be acknowledged:

- NU1902 warnings from transitive identity model packages in test/build output
- Local machine runtime mismatch for direct backend run if ASP.NET Core 8 runtime is missing
  - Workaround: use installed compatible runtime or set DOTNET_ROLL_FORWARD=Major for local runs

## 7) Sign-Off

Complete this section for release records.

- Date:
- Branch/Commit:
- Verified by:
- Result (Go / No-Go):
- Notes:
