# Features

Feature modules representing distinct business capabilities (patients, appointments, treatments, etc.).

Each feature is a self‑contained Angular module with its own:

- **Routes**: Feature‑specific routing
- **Components**: Pages and components specific to the feature
- **Services**: Feature‑specific business logic (may delegate to Core services)
- **State**: Feature‑level state management (if needed)

Features may depend on Core, Shared, and Shell, but not on other feature modules directly.