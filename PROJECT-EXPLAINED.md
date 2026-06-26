# WebApplication8 - Step-by-Step Project Explanation

## 1. Project Purpose
This project is a simple ASP.NET Core 8 web application that combines:
- a server-side backend using **ASP.NET Core MVC** and **Entity Framework Core**
- a SQL Server LocalDB database for data persistence
- a React frontend built with **Vite**
- RabbitMQ messaging for event publishing and consuming

The example shows how to:
- perform CRUD operations against `/api/departments`
- store data in a database
- show live event updates in the UI
- build React code into the ASP.NET app

## 2. Main folders and files

### Root files
- `WebApplication8.sln` — Solution file for the project
- `AGENTS.md` — repository notes (not part of runtime)

### ASP.NET Core app
- `WebApplication8/WebApplication8.csproj` — .NET project file
- `WebApplication8/Program.cs` — application startup, service registration, request pipeline
- `WebApplication8/appsettings.json` — production settings
- `WebApplication8/appsettings.Development.json` — development settings

### Backend code
- `WebApplication8/Controllers/` — MVC and API controllers
  - `DepartmentsController.cs` — department CRUD API
  - `EmployeesController.cs` — employee API
  - `EventsController.cs` — exposes consumed RabbitMQ events
  - `HomeController.cs` — site pages
- `WebApplication8/Data/AppDbContext.cs` — EF Core database context
- `WebApplication8/Models/` — data model classes
  - `Department.cs`
  - `Employee.cs`
  - `Product.cs`
- `WebApplication8/Migrations/` — EF Core database migrations

### Frontend code
- `WebApplication8/ClientApp/package.json` — React package settings
- `WebApplication8/ClientApp/vite.config.js` — Vite build config
- `WebApplication8/ClientApp/src/` — React source files
  - `departments.jsx` — department CRUD page component
  - other React app entrypoints: `main.jsx`, `crud.jsx`, `employees.jsx`

### View pages
- `WebApplication8/Views/Home/Departments.cshtml` — renders the Departments page and mounts React

## 3. How the app starts

1. The app starts from `WebApplication8/Program.cs`.
2. It registers these services:
   - MVC views and controllers with `AddControllersWithViews()`
   - `AppDbContext` for EF Core and SQL Server LocalDB
   - RabbitMQ services: `RabbitMqConnection`, `RabbitMqPublisher`, `EventStore`
   - background services: `EmployeeEventConsumer`, `DepartmentEventConsumer`
3. It sets up the request pipeline:
   - `UseHttpsRedirection()`
   - `UseStaticFiles()`
   - `UseRouting()`
   - `UseAuthorization()`
   - `MapControllerRoute(...)`
4. Then it runs the app with `app.Run()`.

## 4. Database and EF Core

### `AppDbContext.cs`
- This class connects the app to the database.
- It defines tables using `DbSet<T>`:
  - `DbSet<Employee> Employees`
  - `DbSet<Department> Departments`
- It also sets a unique index on `Department.Code`.
- It seeds three initial employees.

### Migrations
- `WebApplication8/Migrations/` contains EF Core migrations.
- These migrations create the database schema.
- To update schema after code changes, use:
  - `dotnet ef migrations add YourMigrationName`
  - `dotnet ef database update`

## 5. Departments API (CRUD)

### `DepartmentsController.cs`
This controller exposes the following API endpoints:

- `GET /api/departments?page=1&pageSize=5`
  - reads a page of departments from the database
  - returns items, total count, page number, and total pages
- `GET /api/departments/{id}`
  - reads one department by id
- `POST /api/departments`
  - creates a new department
  - validates model state and duplicate code
  - publishes a RabbitMQ event after saving
- `PUT /api/departments/{id}`
  - updates an existing department
  - checks duplicate code before save
  - publishes an update event
- `DELETE /api/departments/{id}`
  - removes a department from the database
  - publishes a delete event

## 6. Messaging with RabbitMQ

### `RabbitMqConnection.cs`
- Creates one shared RabbitMQ connection.
- Uses configuration values for host, user, and password.
- Provides `CreateChannelAsync(queueName)` to declare and use a queue.

### `RabbitMqPublisher.cs`
- Sends JSON messages to a queue.
- Called by controllers after database operations.
- Uses a new channel per publish.

### `EventStore.cs`
- Keeps recent consumed events in memory.
- Stores messages grouped by topic.
- The UI polls `GET /api/events?topic=department` to show live messages.

### Consumers
- `RabbitMqEventConsumer.cs` — base class that listens to a queue.
- `EmployeeEventConsumer.cs` — listens to `employee-events` queue.
- `DepartmentEventConsumer.cs` — listens to `department-events` queue.
- Each consumer adds consumed messages into `EventStore`.

## 7. Frontend (React + Vite)

### `ClientApp/package.json`
- `npm run dev` starts the Vite dev server.
- `npm run build` builds the React apps.

### `ClientApp/vite.config.js`
- Builds output into `wwwroot/dist`
- Defines four entrypoints:
  - `react-demo` -> `src/main.jsx`
  - `crud-demo` -> `src/crud.jsx`
  - `employees-demo` -> `src/employees.jsx`
  - `departments-demo` -> `src/departments.jsx`

### `Views/Home/Departments.cshtml`
- Contains a div with `id="departments-root"`.
- Loads `~/dist/departments-demo.js` via a module script.
- That script mounts the React component into the page.

### `ClientApp/src/departments.jsx`
This file does the main frontend work:
- loads a page of departments from `/api/departments`
- shows a form to add or edit a department
- sends `POST`, `PUT`, and `DELETE` requests to the API
- shows live RabbitMQ events by polling `/api/events?topic=department`
- renders pagination buttons

## 8. How to run the app locally

1. Open a terminal in `WebApplication8/WebApplication8/ClientApp`.
2. Install frontend packages:
   ```bash
   npm install
   ```
3. Build the React app:
   ```bash
   npm run build
   ```
4. Run the .NET app from the root of the ASP.NET project:
   ```bash
   dotnet run --project WebApplication8/WebApplication8/WebApplication8.csproj
   ```

### If you want live React development

1. Run `npm run dev` in `WebApplication8/WebApplication8/ClientApp`.
2. Open the React app at `http://localhost:5173` if configured.

## 9. Simple flow summary

1. Browser opens `/Home/Departments`.
2. `Departments.cshtml` loads the React script `departments-demo.js`.
3. React mounts into `<div id="departments-root"></div>`.
4. React calls `/api/departments` to load data.
5. When user adds/edits/deletes, React calls the API.
6. API updates the database and publishes RabbitMQ events.
7. Background consumers read those events and store them.
8. React polls `/api/events?topic=department` and shows new event messages.

## 10. Key files to read first

1. `WebApplication8/Program.cs`
2. `WebApplication8/Data/AppDbContext.cs`
3. `WebApplication8/Controllers/DepartmentsController.cs`
4. `WebApplication8/Views/Home/Departments.cshtml`
5. `WebApplication8/ClientApp/src/departments.jsx`
6. `WebApplication8/Messaging/RabbitMqPublisher.cs`
7. `WebApplication8/Messaging/RabbitMqEventConsumer.cs`

---

If you want, I can also explain just the `Departments` page step by step in Hindi with exactly what each line does.