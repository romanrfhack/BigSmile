# Shell

Shell (application container) responsible for the outermost UI layout, navigation, and global state.

- **Layout components**: Header, footer, sidebars, main content area
- **Navigation**: Routing configuration, guards, route resolvers
- **Global providers**: Singleton services (authentication, tenant context, notification)
- **Root‑level interceptors**: HTTP interceptors, error handlers

The shell may depend on Core and Shared but not on individual feature modules.