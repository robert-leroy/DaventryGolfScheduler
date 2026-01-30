# Golf Tee Time Scheduler

A web application for scheduling weekly golf tee times for a group of neighbors. Admins update the weekly schedule, and users can view and register for available tee times.

This entire project has been developed and managed by Anthropic's Claude Code AI.  I needed a use case to explore and learn more about Vibe Coding.  This has been my best experience yet.  My plan is to continue to maintain and expand this application using Claude.

## Technology Stack

- **Frontend**: Vue 3 (Composition API) + TypeScript + Vite + Pinia
- **Backend**: .NET 8 Web API + Entity Framework Core
- **Database**: PostgreSQL
- **Authentication**: Azure AD B2C via MSAL
- **Hosting**: Azure App Services
- **CI/CD**: GitHub Actions

## Project Structure

```
/golf-scheduler/
├── /src/
│   ├── /GolfScheduler.Api/           # .NET 8 Web API
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Services/
│   │   └── Program.cs
│   └── /golf-scheduler-ui/           # Vue 3 Frontend
│       ├── src/
│       │   ├── components/
│       │   ├── views/
│       │   ├── stores/
│       │   ├── services/
│       │   └── router/
│       └── package.json
├── /.github/
│   └── workflows/
│       ├── api-deploy.yml
│       └── ui-deploy.yml
└── README.md
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Azure AD B2C tenant](https://docs.microsoft.com/en-us/azure/active-directory-b2c/)

## Local Development Setup

### 1. Database Setup

Create a PostgreSQL database:

```sql
CREATE DATABASE golf_scheduler;
```

### 2. Azure AD B2C Configuration

1. Create an Azure AD B2C tenant
2. Register two applications:
   - **SPA (Frontend)**: Public client with redirect URIs for localhost and production
   - **API**: Expose an API scope (e.g., `access_as_user`)
3. Create user flows:
   - Sign up and sign in (B2C_1_signupsignin)
   - Password reset (B2C_1_passwordreset)

### 3. Backend Setup

1. Navigate to the API project:
   ```bash
   cd src/GolfScheduler.Api
   ```

2. Update `appsettings.json` with your configuration:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=golf_scheduler;Username=postgres;Password=your_password"
     },
     "AzureAdB2C": {
       "Instance": "https://your-tenant.b2clogin.com",
       "Domain": "your-tenant.onmicrosoft.com",
       "ClientId": "your-api-client-id",
       "SignUpSignInPolicyId": "B2C_1_signupsignin",
       "TenantId": "your-tenant-id"
     }
   }
   ```

3. Run database migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. Start the API:
   ```bash
   dotnet run
   ```

   The API will be available at `http://localhost:5000`.

### 4. Frontend Setup

1. Navigate to the UI project:
   ```bash
   cd src/golf-scheduler-ui
   ```

2. Copy the environment file and configure:
   ```bash
   cp .env.example .env
   ```

   Update `.env` with your B2C settings:
   ```
   VITE_API_URL=http://localhost:5000
   VITE_B2C_CLIENT_ID=your-spa-client-id
   VITE_B2C_AUTHORITY=https://your-tenant.b2clogin.com/your-tenant.onmicrosoft.com/B2C_1_signupsignin
   VITE_B2C_KNOWN_AUTHORITIES=your-tenant.b2clogin.com
   VITE_B2C_REDIRECT_URI=http://localhost:5173
   VITE_B2C_SCOPES=https://your-tenant.onmicrosoft.com/api/access_as_user
   ```

3. Install dependencies:
   ```bash
   npm install
   ```

4. Start the development server:
   ```bash
   npm run dev
   ```

   The frontend will be available at `http://localhost:5173`.

## API Endpoints

### Tee Times
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | /api/teetimes | List upcoming tee times | User |
| GET | /api/teetimes/{id} | Get tee time details | User |
| POST | /api/teetimes | Create tee time | Admin |
| PUT | /api/teetimes/{id} | Update tee time | Admin |
| DELETE | /api/teetimes/{id} | Delete tee time | Admin |

### Registrations
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | /api/teetimes/{id}/register | Register for tee time | User |
| DELETE | /api/teetimes/{id}/register | Cancel registration | User |
| GET | /api/registrations/me | Get user's registrations | User |

### Users (Admin)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | /api/users | List all users | Admin |
| PUT | /api/users/{id}/admin | Toggle admin status | Admin |
| GET | /api/users/me | Get current user profile | User |

## Deployment

### GitHub Actions

The project includes two GitHub Actions workflows:

1. **api-deploy.yml**: Builds and deploys the .NET API to Azure App Service
2. **ui-deploy.yml**: Builds and deploys the Vue frontend to Azure App Service

### Required Secrets and Variables

Set these in your GitHub repository settings:

**Secrets:**
- `AZURE_API_PUBLISH_PROFILE`: Azure publish profile for the API App Service
- `AZURE_UI_PUBLISH_PROFILE`: Azure publish profile for the UI App Service

**Variables:**
- `VITE_API_URL`: Production API URL
- `VITE_B2C_CLIENT_ID`: B2C SPA client ID
- `VITE_B2C_AUTHORITY`: B2C authority URL
- `VITE_B2C_KNOWN_AUTHORITIES`: B2C known authorities
- `VITE_B2C_REDIRECT_URI`: Production redirect URI
- `VITE_B2C_SCOPES`: API scopes

### Azure App Service Configuration

Configure these settings in your Azure App Services:

**API App Service:**
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `AzureAdB2C__Instance`: B2C instance URL
- `AzureAdB2C__Domain`: B2C domain
- `AzureAdB2C__ClientId`: API client ID
- `AzureAdB2C__SignUpSignInPolicyId`: Sign-in policy name
- `FrontendUrl`: Production frontend URL (for CORS)

## Making the First User an Admin

After the first user signs up, you'll need to manually set them as an admin in the database:

```sql
UPDATE users SET is_admin = true WHERE email = 'first-user@example.com';
```

Once you have an admin user, they can promote other users to admin through the UI.

## License

MIT
