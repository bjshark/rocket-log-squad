# Architecture

## Authentication Architecture

### Strategy
Use JWT bearer tokens issued by the ASP.NET Core API. This keeps the API stateless and decoupled from any specific identity provider.

### Dev/Demo Bypass
In `appsettings.Development.json`, set `"Auth": { "Enabled": false }`. A custom middleware component checks this flag. When disabled, it injects a fixed dev identity (e.g., a seeded admin user) into `HttpContext.User` and short-circuits normal token validation. No login is required in this mode, making the app fully functional for development and demo purposes without any auth infrastructure.

### Production Auth Path (two options — decide later)
**Option A — Local Identity:** Use ASP.NET Core Identity to manage users in MongoDB (or a side SQL database). Issue JWTs from the API directly. Add email/password login UI to the React app. Simple and self-contained.

**Option B — External OAuth/OIDC:** Integrate OpenIddict or delegate to a provider (Google, Microsoft, Auth0). The React app performs the OAuth flow and exchanges the authorization code for a token. The API validates the token against the provider's public keys. This is the more scalable, production-grade approach.

Both options share the same JWT bearer contract in the API — only the token issuer and signing key differ. The React app's API calls do not change between options.

---

## Frontend Architecture

### Project Structure
```
/src
  /api              → API client functions (grouped by resource)
  /components       → Shared/reusable UI components
  /pages            → Top-level route components
    /inventory      → Rockets, Engines, Accessories
    /launches       → Launch log list + detail + new launch form
    /catalog        → Browse master rockets/engines
    /settings       → User preferences
  /hooks            → Custom React hooks (useGeolocation, useWeather, etc.)
  /context          → Auth context, app-wide state
  /utils            → Helpers, formatters
  /assets           → Static images, icons
```

### Key Pages / Views
- **Dashboard** — Recent launches, quick stats (total launches, rocket count), quick-launch button
- **My Rockets** — Card grid of user's rockets with build photo, condition badge, launch count
- **My Engines** — Inventory table with quantity-on-hand, quick increment/decrement
- **Launch Log** — Chronological list of launches; tap to view detail
- **New Launch Form** — The primary field-use form: select rocket, select engine, auto-capture location + weather, notes, photo upload, outcome
- **Master Catalog Browser** — Browse/search rockets and engines from the master list; add to personal inventory from here
- **Admin Panel** — Simple CRUD for master rockets/engines (role-gated, you and your son's admin account only)

### PWA Configuration
Use `vite-plugin-pwa` with Workbox in `GenerateSW` mode. Cache strategy:
- **Shell / static assets:** Cache-first
- **API GET requests (master catalog):** Stale-while-revalidate (data changes rarely)
- **API GET requests (user data):** Network-first with fallback to cache (so recently viewed data is readable offline)
- **No offline writes** in v1 — if offline, the New Launch form shows a warning and disables submission

### Geolocation & Weather Flow (New Launch Form)
1. User taps "New Launch"
2. App calls `navigator.geolocation.getCurrentPosition()`
3. On success, lat/lng are stored in form state and displayed as a map pin or coordinate readout
4. App calls `GET /api/v1/weather?lat=...&lng=...` to fetch and display current conditions
5. User reviews weather (pre-filled), can manually override any field
6. On form submit, location + weather snapshot are sent as part of the launch payload
7. Reverse-geocoded location name (from API) is stored alongside raw coordinates

---