# Asset Management SQL Assets

This folder contains reference SQL assets for the asset management module.

- `001_create_asset_management_tables.sql`: table and index script mirroring the module's EF model
- `002_seed_asset_management_defaults.sql`: default seed data for locations, categories, and workflow definitions

Use these scripts as documentation or for DBA review. The primary migration path for the solution remains EF Core migrations through `AppDbContext`.
