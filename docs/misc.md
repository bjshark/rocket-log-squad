## Environment Configuration

### API (`appsettings.json` / environment variables)
```
MongoDB__ConnectionString
MongoDB__DatabaseName
Jwt__Issuer
Jwt__Audience
Jwt__SecretKey
Jwt__ExpiryMinutes
Auth__Enabled                  → false in Development
Storage__Provider              → "local" | "azure" | "s3"
Storage__LocalPath             → path for local file storage in dev
OpenWeatherMap__ApiKey
Cors__AllowedOrigins
Cors__AllowCredentials         → false by default (set true only when using cookies/credentials)
```

`Cors__AllowedOrigins` may be configured as a JSON array in `appsettings*.json` or as a comma/semicolon-separated environment variable value, for example:

`Cors__AllowedOrigins=http://localhost:5173,http://127.0.0.1:5173,http://localhost:4173`

### Frontend (`.env` / `.env.local`)
```
VITE_API_BASE_URL
VITE_ENABLE_AUTH               → false for dev/demo
```

For local development with Vite, set `VITE_API_BASE_URL` to `http://localhost:8080` when the backend is running on the host or Docker with `8080:8080` mapped.

### Local Run Quick Guide

From repository root:

```bash
# Backend API
dotnet restore rocket-log.sln
dotnet build rocket-log.sln
dotnet run --project backend/RocketLog.Api.csproj

# Frontend app
npm --prefix frontend install
npm --prefix frontend run dev
```

Validation commands:

```bash
# Frontend production build
npm --prefix frontend run build

# Integration tests (Mongo2Go)
dotnet test tests/RocketLog.Api.IntegrationTests/RocketLog.Api.IntegrationTests.csproj
```

### Local Troubleshooting

- Vite dev port shifts and CORS: if Vite starts on a different port (for example 5174), add that origin to `backend/appsettings.Development.json` under `Cors:AllowedOrigins` or restart Vite on an allowed port.
- API base URL mismatch: make sure `VITE_API_BASE_URL` matches the running backend URL (default local mapping is `http://localhost:8080`).
- Docker backend stale image: after backend code/config changes, rebuild with `docker-compose up -d --build backend`.
- Docker full reset when needed: run `docker-compose down` then `docker-compose up -d --build`.

### Final Release Readiness

Use `docs/release-readiness-checklist.md` as the go/no-go artifact before handoff or deployment.

---

## Key Third-Party Services

| Service | Purpose | Notes |
|---|---|---|
| OpenWeatherMap | Weather at launch time | Free tier: 60 calls/min, 1M calls/month |
| Nominatim (OpenStreetMap) | Reverse geocoding (lat/lng → location name) | Free, no key required; rate limit: 1 req/sec |
| MongoDB Atlas | Managed MongoDB in production | Free tier (512MB) sufficient for personal use |
| Azure Blob / AWS S3 / Cloudflare R2 | Image storage in production | R2 has no egress fees — good choice for images |
| Vercel / Netlify / Azure Static Web Apps | Frontend hosting | All have free tiers |

---