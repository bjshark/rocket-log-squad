# Rocket Log

A Progressive Web App (PWA) for hobbyist model rocketry enthusiasts. Manage your rocket and engine inventory, log launches with weather and GPS data, and keep a photo journal — all from your phone in the field or at your desktop at home.

---

## Features

- **Inventory** — Track rockets you own/built, motors/engines on hand, and accessories
- **Launch Log** — Log launches with GPS location, weather snapshot, rocket + engine used, outcome, notes, and an optional photo; supports retroactive logging with historical weather lookup
- **Master Catalog** — Browse a curated library of rocket models and engine specs; add items directly to your personal inventory
- **Photos** — Attach a build photo per rocket and an optional photo per launch entry
- **PWA** — Installable on iOS and Android; recently-viewed data is readable offline; new launch form warns and disables submission when offline
- **Admin Panel** — Role-gated CRUD for the master catalog (developer/admin accounts only)
- **Multi-User** — Supports multiple user accounts; auth is fully bypassable in dev/demo mode

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript + Vite 7 |
| UI | React-Bootstrap / Bootstrap 5 |
| Forms | React Hook Form |
| Routing | React Router v6 |
| PWA | vite-plugin-pwa + Workbox |
| Backend | ASP.NET Core Web API (.NET 8) |
| Database | MongoDB (`MongoDB.Driver`) |
| Auth | JWT bearer (bypassed in dev via middleware flag) |
| Tests | xUnit + Mongo2Go (in-process MongoDB) |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or .NET 10 — project rolls forward)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recommended for local MongoDB)

---

### Option A — Docker backend (recommended)

Starts MongoDB and the API together:

```bash
docker compose up -d
```

The API will be available at `http://localhost:8080`. MongoDB runs on the default port `27017`.

Then start the frontend:

```bash
npm --prefix frontend install
npm --prefix frontend run dev
```

The app opens at `http://localhost:5173` (or the next available port).

---

### Option B — Run everything directly

```bash
# Restore and run the API
dotnet restore rocket-log.sln
dotnet run --project backend/RocketLog.Api.csproj

# In a separate terminal, start the frontend
npm --prefix frontend install
npm --prefix frontend run dev
```

> **Note:** Direct `dotnet run` requires ASP.NET Core 8 runtime. If only .NET 10 is installed, prefix with `DOTNET_ROLL_FORWARD=Major` or use the Docker backend instead.

---

## Environment Configuration

### Backend (`appsettings.json` / environment variables)

| Variable | Description |
|----------|-------------|
| `MongoDB__ConnectionString` | MongoDB connection string |
| `MongoDB__DatabaseName` | Database name (default: `rocket_log`) |
| `Auth__Enabled` | `false` in Development to bypass token validation |
| `Jwt__SecretKey` | JWT signing key (production) |
| `Jwt__Issuer` / `Jwt__Audience` | JWT issuer/audience (production) |
| `OpenWeatherMap__ApiKey` | Weather API key (optional; stub fallback used if absent) |
| `Cors__AllowedOrigins` | Comma-separated list of allowed frontend origins |
| `Storage__Provider` | `local` \| `azure` \| `s3` |

### Frontend (`.env.local`)

```env
VITE_API_BASE_URL=http://localhost:8080
VITE_ENABLE_AUTH=false
```

---

## Running Tests

Integration tests use [Mongo2Go](https://github.com/Mongo2Go/Mongo2Go) to spin up an in-process MongoDB instance — no external database required.

```bash
dotnet test tests/RocketLog.Api.IntegrationTests/RocketLog.Api.IntegrationTests.csproj
```

---

## Project Structure

```
rocket-log/
├── backend/                  # ASP.NET Core Web API
│   ├── Controllers/          # Route handlers
│   ├── Services/             # Business logic (weather, launches, etc.)
│   ├── Models/               # DTOs and domain models
│   ├── Data/                 # MongoDB context and repositories
│   ├── Middleware/           # Auth bypass, error handling
│   └── Seeders/              # Master catalog data seeder
├── frontend/                 # React + Vite SPA
│   └── src/
│       ├── api/              # API client functions by resource
│       ├── components/       # Shared UI components
│       ├── pages/            # Route-level page components
│       │   ├── catalog/      # Master catalog browser
│       │   ├── inventory/    # My rockets, engines, accessories
│       │   ├── launches/     # Launch log
│       │   └── admin/        # Admin catalog management
│       ├── hooks/            # Custom React hooks
│       └── context/          # Auth context and app state
├── tests/
│   └── RocketLog.Api.IntegrationTests/
├── docs/                     # Specification, architecture, and API docs
├── docker-compose.yml
└── rocket-log.sln
```

---

## Documentation

| Doc | Description |
|-----|-------------|
| [docs/overview.md](docs/overview.md) | Goals, non-goals, and feature scope |
| [docs/architecture.md](docs/architecture.md) | System design, auth strategy, PWA config |
| [docs/tech-stack.md](docs/tech-stack.md) | Framework and library decisions |
| [docs/data-model.md](docs/data-model.md) | MongoDB schema |
| [docs/api-spec.md](docs/api-spec.md) | REST API reference |
| [docs/phases.md](docs/phases.md) | Build phases and delivery order |
| [docs/misc.md](docs/misc.md) | Local run guide and troubleshooting |
| [docs/release-readiness-checklist.md](docs/release-readiness-checklist.md) | Go/no-go checklist before deployment |

---

## Troubleshooting

**Vite starts on a different port (e.g., 5174) and API calls fail with CORS errors**
Add the new port to `Cors:AllowedOrigins` in `backend/appsettings.Development.json`, or stop all Vite instances and restart so it claims port 5173.

**`VITE_API_BASE_URL` mismatch**
Ensure `.env.local` points to the running backend. Default when using Docker: `http://localhost:8080`.

**Docker backend shows stale behavior after a code change**
Rebuild the container: `docker compose up -d --build backend`

**`dotnet run` fails with runtime not found**
See Option A (Docker) or prefix the command with `DOTNET_ROLL_FORWARD=Major`.
