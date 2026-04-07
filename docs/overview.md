# Project Overview

A Progressive Web App (PWA) for hobbyist model rocketry enthusiasts to manage their rocket and engine inventory, log launches, track weather and location, and attach photos. The app is designed to be mobile-first and fully responsive, usable in the field on a phone and at home on a desktop or tablet. It is a personal/family-scale application with support for multiple user accounts.

Final release gate: use `docs/release-readiness-checklist.md` for go/no-go verification before handoff or deployment.

---

## Goals & Non-Goals

### Goals
- Inventory management: rockets owned/built, motors/engines, accessories
- Launch log/journal with per-launch weather, GPS location, rocket+engine used, notes, and optional photos - with ability to log a launch retroactively and get the weather given a past date/time and location 
- Curated master dataset of rocket models and engine specs (managed by the developer)
- PWA with offline-friendly caching (basic — no full offline write support required initially)
- Multi-user support with auth that is bypassable in dev/demo mode
- Fully responsive UI: mobile-first, usable on tablet and desktop
- Photo support: build photo per rocket, optional photo per launch log entry
- Master data images (e.g., box art / product thumbnails) for rockets and engines

### Non-Goals (initial version)
- Full offline write/sync (no IndexedDB-backed queue for now)
- Social/sharing features
- Integration with NAR/TRA certification tracking
- Native mobile app (PWA only)

---