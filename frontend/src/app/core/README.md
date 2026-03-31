# Core

Core domain‑level services, models, and utilities that are essential for the application and independent of the UI layer.

- **Models**: TypeScript interfaces/classes representing domain entities (Patient, Appointment, etc.)
- **Services**: Application‑level business logic (API clients, state management, validation)
- **Utilities**: Pure helper functions (date formatting, string manipulation, etc.)

This layer must not depend on any Angular‑specific code (components, directives, pipes) or feature modules.