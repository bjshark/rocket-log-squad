# Data Model

All collections live in a single MongoDB database (e.g., `rocketry_db`).

### `users`
```
{
  _id: ObjectId,
  email: string (unique),
  displayName: string,
  passwordHash: string,           // if using local identity; omit if OAuth-only
  oauthProvider: string | null,   // "google", "microsoft", etc.
  oauthSubject: string | null,
  roles: string[],                // ["user", "admin"]
  preferences: {
    units: "imperial" | "metric",
    defaultLaunchSite: string | null
  },
  createdAt: DateTime,
  updatedAt: DateTime
}
```

### `rockets` (master catalog — developer-managed)
```
{
  _id: ObjectId,
  manufacturer: string,           // "Estes", "Quest", "Apogee", etc.
  sku: string,                    // manufacturer SKU/kit number
  name: string,                   // "Alpha III", "Big Bertha", etc.
  description: string,
  skillLevel: string,             // "Beginner", "Intermediate", "Expert"
  recommendedEngines: string[],   // ["B6-4", "C6-5"]
  diameter_mm: number,
  length_mm: number,
  weight_g: number,               // without engine
  finMaterial: string,
  noseCone: string,
  recoverySystem: string,         // "Parachute", "Streamer"
  thumbnailUrl: string,           // CDN/storage URL to box art or product image
  imageUrls: string[],
  productUrl: string | null,      // link to manufacturer/retailer page
  isActive: boolean,
  createdAt: DateTime,
  updatedAt: DateTime
}
```

### `engines` (master catalog — developer-managed)
```
{
  _id: ObjectId,
  manufacturer: string,           // "Estes", "AeroTech", "Quest"
  designation: string,            // "B6-4" (the standard motor code)
  impulseClass: string,           // "B"
  totalImpulse_Ns: number,
  averageThrust_N: number,
  delay_s: number,
  diameter_mm: number,
  length_mm: number,
  propellantWeight_g: number,
  totalWeight_g: number,
  caseType: string,               // "Single-use", "Reloadable"
  propellantType: string,         // "Black powder", "Composite"
  thumbnailUrl: string,
  imageUrls: string[],
  certificationBody: string | null, // "NAR", "TRA", null
  isActive: boolean,
  createdAt: DateTime,
  updatedAt: DateTime
}
```

### `user_rockets` (user's personal inventory of built/owned rockets)
```
{
  _id: ObjectId,
  userId: ObjectId,               // ref: users
  rocketId: ObjectId,             // ref: rockets (master catalog)
  nickname: string | null,        // user's personal name for this build
  buildDate: Date | null,
  condition: "New" | "Good" | "Fair" | "Retired",
  buildNotes: string | null,
  photoUrl: string | null,        // user's build photo
  createdAt: DateTime,
  updatedAt: DateTime
}
```

### `user_engines` (user's engine inventory)
```
{
  _id: ObjectId,
  userId: ObjectId,               // ref: users
  engineId: ObjectId,             // ref: engines (master catalog)
  quantityOnHand: number,
  purchaseDate: Date | null,
  notes: string | null,
  updatedAt: DateTime
}
```

### `launches`
```
{
  _id: ObjectId,
  userId: ObjectId,
  userRocketId: ObjectId,         // ref: user_rockets
  engineId: ObjectId,             // ref: engines (master catalog)
  launchDate: DateTime,
  location: {
    name: string | null,          // reverse-geocoded or user-entered
    lat: number,
    lng: number
  },
  weather: {
    source: "api" | "manual",
    temperatureF: number,
    windSpeedMph: number,
    windDirection: string,        // "NNW", etc.
    humidity: number,
    conditions: string,           // "Clear", "Partly Cloudy", etc.
    visibility_mi: number | null
  },
  outcome: "Success" | "Partial" | "Failure" | "No Launch",
  altitudeFt: number | null,      // estimated or measured
  notes: string | null,
  photoUrl: string | null,
  createdAt: DateTime,
  updatedAt: DateTime
}
```

### `accessories` (optional — user-owned accessories)
```
{
  _id: ObjectId,
  userId: ObjectId,
  name: string,                   // "Launch Controller", "Launch Pad", etc.
  category: string,
  brand: string | null,
  notes: string | null,
  photoUrl: string | null,
  createdAt: DateTime,
  updatedAt: DateTime
}
```

---