# Golf Scheduler

A full-stack golf tee time scheduling application that allows users to view available tee times and register for them.

## Project Structure

```
golf-scheduler/
├── golf-scheduler.sln           # Solution file
└── src/
    ├── GolfScheduler.Api/       # ASP.NET Core Web API backend
    └── golf-scheduler-ui/       # Vue 3 SPA frontend
```

## Tech Stack

### Backend (GolfScheduler.Api)
- **Framework**: ASP.NET Core 10.0
- **Database**: PostgreSQL with Entity Framework Core 10.0
- **Authentication**: Azure AD B2C (with development bypass mode)
- **API Documentation**: Swagger/OpenAPI

### Frontend (golf-scheduler-ui)
- **Framework**: Vue 3 with Composition API
- **Language**: TypeScript
- **Build Tool**: Vite
- **State Management**: Pinia
- **Routing**: Vue Router
- **HTTP Client**: Axios
- **Authentication**: MSAL Browser (@azure/msal-browser)

## Domain Model

### Core Entities
- **User**: Golfers with profile info (email, name, phone, handicap, admin flag)
- **TeeTime**: Scheduled tee times with date, time, max players, and notes
- **Registration**: Links users to tee times they've signed up for

### Key Relationships
- Users can create tee times (admin only)
- Users can register for tee times
- Each tee time has a max player limit (default: 4)
- Registration is unique per user per tee time

## API Endpoints

### Tee Times (`/api/teetimes`)
- `GET /` - List upcoming tee times with availability
- `GET /by-day` - Get tee times grouped by day with registered golfers
- `GET /{id}` - Get tee time details with registrations
- `POST /` - Create tee time (admin only)
- `PUT /{id}` - Update tee time (admin only)
- `DELETE /{id}` - Delete tee time (admin only)
- `POST /{id}/register` - Register current user for tee time
- `DELETE /{id}/register` - Cancel registration

### Users (`/api/users`)
- User profile management endpoints

### Registrations (`/api/registrations`)
- Registration management endpoints

## Development

### Running the Backend
```bash
cd golf-scheduler/src/GolfScheduler.Api
dotnet run
```

The API runs on `https://localhost:5001` (or configured port) with Swagger UI available at `/swagger`.

### Running the Frontend
```bash
cd golf-scheduler/src/golf-scheduler-ui
npm install
npm run dev
```

The UI runs on `http://localhost:5173` by default.

### Development Authentication
Set `BypassAuthentication: true` in appsettings to use a dev user without Azure AD B2C.

### Database
PostgreSQL connection string is configured in `appsettings.json` under `ConnectionStrings:DefaultConnection`.

Migrations are auto-applied in development mode on startup.

## Key Files

### Backend
- `Program.cs` - Application entry point and service configuration
- `Data/AppDbContext.cs` - EF Core database context
- `Models/` - Entity definitions (User, TeeTime, Registration)
- `Controllers/` - API controllers
- `DTOs/` - Data transfer objects
- `Services/` - Business logic services

### Frontend
- `src/main.ts` - Application entry point
- `src/App.vue` - Root component
- `src/router/index.ts` - Route definitions
- `src/stores/` - Pinia stores (auth, teeTime, registration)
- `src/services/` - API and auth services
- `src/views/` - Page components
- `src/components/` - Reusable components
