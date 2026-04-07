# Recommended Tech Stack

### Frontend
**React 18+ with Vite**

Rationale: Vite is the current standard (CRA is deprecated). React's component ecosystem, hooks model, and community support make it the strongest choice for a PWA of this scale. Your existing jQuery/Bootstrap background transfers well — think of React components as reusable "widgets" with co-located logic.

- **UI / Design System:** React Bootstrap (`react-bootstrap`) or Bootstrap 5 with CSS Modules. This keeps your existing Bootstrap mental model intact while giving you React-idiomatic component usage. Alternatively, consider **Mantine** (Bootstrap-like API, excellent mobile defaults, great form components) if you want something more modern out of the box.
- **Routing:** React Router v6
- **State Management:** React Context + `useReducer` for local/global UI state. If state grows complex, **Zustand** is lightweight and approachable.
- **Forms:** React Hook Form (performant, minimal re-renders, good validation API)
- **PWA:** Vite PWA Plugin (`vite-plugin-pwa`) — handles service worker generation, manifest, and asset caching via Workbox automatically
- **HTTP Client:** Axios or native `fetch` with a thin wrapper
- **Image Handling:** Browser native `<input type="file" accept="image/*" capture="environment">` for camera access on mobile; upload via multipart form to the API

### Backend / API
**ASP.NET Core (C# / .NET 8+)**

Rationale: You have deep expertise here, it will be the fastest path to a solid API. .NET 8 minimal APIs are clean and concise; controller-based is fine too given your background.

- **Framework:** ASP.NET Core Web API (.NET 8)
- **MongoDB Driver:** Official `MongoDB.Driver` NuGet package
- **Auth:** ASP.NET Core Identity with JWT bearer tokens for API auth. In dev/demo mode, a flag in `appsettings.Development.json` (`Auth:Enabled: false`) bypasses token validation — middleware checks this flag and short-circuits to an anonymous identity. This keeps OAuth integration optional and non-blocking during development.
- **OAuth / OpenID:** When ready, integrate via OpenIddict (which you're familiar with) or delegate to an external provider (Google, Microsoft, Auth0) via ASP.NET Core's built-in OAuth middleware. The JWT-based API contract does not change — only the token issuer changes.
- **Image Storage:** Store uploaded images in a local filesystem path (dev) or cloud blob storage (prod — e.g., Azure Blob Storage or AWS S3). Return public URLs in API responses. Do not store binary blobs in MongoDB.
- **Weather:** OpenWeatherMap API (free tier is sufficient). The client captures GPS coordinates and passes them to the API at launch-log creation time; the API fetches and stores the weather snapshot inline on the launch document.
- **Geolocation:** Browser Geolocation API on the client captures lat/lng. Optionally reverse-geocode to a human-readable location name using OpenWeatherMap's geo endpoint or a free service like Nominatim (OpenStreetMap).

### Database
**MongoDB** (as specified)

- Use a single database with the following collections (see Data Models section)
- Connection via `MongoDB.Driver` with `IMongoClient` registered as a singleton in DI
- For local dev: MongoDB Community Edition or Docker (`mongo:7` image)
- For prod: MongoDB Atlas free tier (512MB) is sufficient for a personal app and eliminates ops overhead

### Hosting (Recommendation)
Given the undecided status, a pragmatic path:

- **Dev/Demo:** Run everything locally. Docker Compose with containers for the API, MongoDB, and (optionally) a reverse proxy.
- **Production Option A (Low cost):** Render.com — free/cheap tier for the API container, MongoDB Atlas free tier. Simple, no infra management.
- **Production Option B (Azure):** Azure App Service (API) + Azure Cosmos DB for MongoDB API (drop-in MongoDB replacement) + Azure Blob Storage (images). A natural fit if you're already in the Microsoft ecosystem.
- **Frontend:** Deploy as a static site to Vercel, Netlify, or Azure Static Web Apps. Vite builds a static bundle; the PWA service worker handles caching from there.

---