# Development Phases

### Phase 0 — Project Setup
- Initialize Vite + React project with chosen UI library
- Initialize ASP.NET Core Web API project
- Set up Docker Compose (API + MongoDB)
- Configure `vite-plugin-pwa` and verify manifest/service worker
- Set up Git repository with a monorepo structure or two separate repos (one per layer)
- Scaffold folder structures for both projects

### Phase 1 — Auth & User Shell
- Implement JWT auth with dev bypass middleware
- Build login/register pages (or stub them with a hard-coded dev session)
- Implement auth context in React; protect routes
- Navigation shell: mobile-first bottom nav bar + desktop sidebar/top nav

### Phase 2 — Master Catalog
- Create seed JSON for 20–30 rockets and key engine lines (Estes A–D range as a start)
- Implement DataSeeder in the API
- Build catalog browse/search pages in React
- Implement catalog API endpoints

### Phase 3 — User Inventory
- My Rockets: add from catalog, nickname, build date, condition, build photo upload
- My Engines: add from catalog, set quantity, adjust on-hand count
- Accessories: free-form add/edit

### Phase 4 — Launch Logging
- New Launch form with rocket/engine selector, geolocation, weather auto-fetch, notes, outcome, photo
- Launch log list and detail view
- Edit and delete existing launches

### Phase 5 — PWA Polish & Offline
- Refine Workbox caching strategy
- Manifest icons, splash screens, theme color
- Offline warning banner on New Launch form
- Test install-to-home-screen on iOS and Android

### Phase 6 — Admin & Data Management
- Role-gated admin panel for CRUD on master rockets/engines
- Image upload for master catalog items
- Export seed JSON from current DB state

---