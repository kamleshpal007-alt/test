# AGENTS

## Repository overview
- This is an ASP.NET Core 8 web application located under `WebApplication8/WebApplication8`.
- The project combines server-side MVC/Razor pages with a Vite-powered React client in `WebApplication8/WebApplication8/ClientApp`.
- Key backend folders: `Controllers/`, `Data/`, `Messaging/`, `Models/`, `Migrations/`, `Views/`, `wwwroot/`.

## Build and run
- Primary build command: `dotnet build WebApplication8/WebApplication8/WebApplication8.csproj`
  - The project file is configured to run `npm install` and `npm run build` for the React client before .NET build.
- Run locally with: `dotnet run --project WebApplication8/WebApplication8/WebApplication8.csproj`
- Client-only development: `npm install` then `npm run dev` from `WebApplication8/WebApplication8/ClientApp`.

## Important conventions
- `ClientApp` is not compiled as part of the .NET app, but its build output is deployed to `WebApplication8/wwwroot/dist`.
- `WebApplication8.csproj` uses `SpaRoot` and custom MSBuild targets to keep the React app in sync with `.NET` builds.
- Do not assume the React files are independent of the .NET project: changes to `package.json` and Vite config affect both build and runtime.

## Database and messaging
- EF Core is configured in `Program.cs` with a SQL Server LocalDB connection string from `appsettings.json`.
- Migrations live in `Migrations/` and should be updated when data model classes change.
- RabbitMQ support is implemented in `Messaging/` with a singleton connection, publisher, event store, and hosted consumer.
  - Relevant files: `RabbitMqConnection.cs`, `RabbitMqPublisher.cs`, `EventStore.cs`, `EmployeeEventConsumer.cs`.

## Frontend structure
- React sources are in `ClientApp/src/`.
- Vite build entries are defined in `ClientApp/vite.config.js`: `main.jsx`, `crud.jsx`, `employees.jsx`.
- Built assets are emitted to `wwwroot/dist` with stable filenames so Razor views can reference them directly.

## When writing or reviewing tasks
- Prefer `.NET` project entrypoints and controllers for API changes.
- Preserve messaging and hosted service registration patterns in `Program.cs`.
- For UI changes, confirm whether they belong in React sources (`ClientApp/src`) or Razor views (`Views/`).
- Keep build-related files and paths consistent with the existing `WebApplication8.csproj` and `vite.config.js` conventions.

## Notes for AI agents
- There is currently no project README or existing Copilot instructions file.
- Use this file as the canonical quick-start and code boundary reference for this repository.
