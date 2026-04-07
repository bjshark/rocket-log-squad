# API Design

RESTful JSON API. All routes prefixed with `/api/v1/`.

### Auth
```
POST   /api/v1/auth/register
POST   /api/v1/auth/login           → returns JWT
POST   /api/v1/auth/refresh
POST   /api/v1/auth/logout
```

### Master Catalog (read-only to users)
```
GET    /api/v1/rockets              → paginated, filterable by manufacturer/name
GET    /api/v1/rockets/{id}
GET    /api/v1/engines              → paginated, filterable by class/manufacturer
GET    /api/v1/engines/{id}
```

### Admin (master data management — role-gated)
```
POST   /api/v1/admin/rockets
PUT    /api/v1/admin/rockets/{id}
DELETE /api/v1/admin/rockets/{id}
POST   /api/v1/admin/engines
PUT    /api/v1/admin/engines/{id}
DELETE /api/v1/admin/engines/{id}
POST   /api/v1/admin/images/upload  → returns CDN/storage URL
```

### User Rocket Inventory
```
GET    /api/v1/my/rockets
POST   /api/v1/my/rockets
GET    /api/v1/my/rockets/{id}
PUT    /api/v1/my/rockets/{id}
DELETE /api/v1/my/rockets/{id}
POST   /api/v1/my/rockets/{id}/photo  → multipart upload
```

### User Engine Inventory
```
GET    /api/v1/my/engines
POST   /api/v1/my/engines
PUT    /api/v1/my/engines/{id}        → update quantity, notes
DELETE /api/v1/my/engines/{id}
```

### Launches
```
GET    /api/v1/my/launches            → paginated, sortable by date
POST   /api/v1/my/launches
GET    /api/v1/my/launches/{id}
PUT    /api/v1/my/launches/{id}
DELETE /api/v1/my/launches/{id}
POST   /api/v1/my/launches/{id}/photo → multipart upload
```

### Weather (utility endpoint called by client at launch-log time)
```
GET    /api/v1/weather?lat={lat}&lng={lng}   → returns weather snapshot object
```

### Accessories
```
GET    /api/v1/my/accessories
POST   /api/v1/my/accessories
PUT    /api/v1/my/accessories/{id}
DELETE /api/v1/my/accessories/{id}
```

---