# Master Data Strategy

### Recommendation: Seed Files in the Repository (Version-Controlled JSON)
Since the dataset is small and slow-changing, manage master rocket and engine data as JSON seed files committed to the repository. On startup (or via a CLI command), the API seeds/upserts these records into MongoDB.

Benefits:
- Full version history of master data changes in Git
- Easy to diff, review, and roll back
- No admin UI dependency for data management in early phases
- Trivially portable — seed data works with any MongoDB instance

Structure:
```
/data
  /seeds
    rockets.json       → array of rocket objects
    engines.json       → array of engine objects
```

The API project includes a `DataSeeder` hosted service or a CLI command (`dotnet run --seed`) that upserts from these files on startup in dev, or on demand in prod.

Later, once the dataset is stable, add the Admin UI for in-place edits — the admin panel writes back to MongoDB, and you periodically export to keep the seed files in sync.

### Image Assets for Master Data
Store master data images (box art, product photos) in the same blob storage used for user uploads, under a separate path prefix (e.g., `/catalog/rockets/`, `/catalog/engines/`). Reference them by URL in the seed JSON. Sourcing options:
- Manufacturer websites (Estes, Quest, Apogee) — download and host yourself to avoid broken external links
- The Estes and Quest ecommerce sites are small enough that you can manually curate a clean image set
- For engine images, the MSDS/spec sheets often have product photos; also check rocketreviews.com

---