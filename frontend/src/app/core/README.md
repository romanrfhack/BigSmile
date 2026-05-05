# Core

Application-level services, models, and utilities that are essential for the frontend and independent of feature UI.

- **Models**: TypeScript interfaces/classes representing domain entities (Patient, Appointment, etc.)
- **Services**: Application‑level business logic (API clients, state management, validation, localization)
- **Utilities**: Pure helper functions (date formatting, string manipulation, etc.)

This layer must not contain components, directives, pipes, or feature-specific orchestration.

The localization foundation lives under `core/i18n`. It defaults to `es-MX`, keeps `en-US` available, and persists only the non-sensitive `bigsmile.ui.language` preference.
