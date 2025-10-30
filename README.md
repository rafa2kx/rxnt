# RXNT Clinic Manager

A full-stack application for managing clinic operations, built with Angular 16, .NET 8 API, and SQL Server, all containerized with Docker.

## Architecture

- **Frontend**: Angular 16 with modern UI/UX
- **Backend**: .NET 8 Web API
- **Database**: SQL Server 2022
- **Containerization**: Docker with multi-container setup

## Prerequisites

- Docker Desktop (Windows/Mac/Linux) - **Required**
- Docker Compose (comes with Docker Desktop)
- Git (optional)

For local development without Docker:
- .NET 8 SDK
- Node.js 18+
- Angular CLI 16

## Quick Start

### Using Docker (Recommended)

The easiest way to get started is using Docker. All services are pre-configured and will start automatically.

```bash
# 1. Start all services
docker compose up -d --build

# 2. Access the application
# Frontend: http://localhost:4200
# API:      http://localhost:5000
# Swagger:  http://localhost:5000/swagger
# Hangfire: http://localhost:5000/hangfire/
```

For detailed setup instructions, see [SETUP.md](SETUP.md).

### Local Development (Without Docker)

#### Backend Setup

1. Navigate to the API directory:
   ```bash
   cd api
   ```

2. Update the connection string in `appsettings.json`

3. Run the API:
   ```bash
   dotnet run
   ```

#### Frontend Setup

1. Navigate to the app directory:
   ```bash
   cd app
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run the Angular app:
   ```bash
   npm start
   ```

## Project Structure

```
RXNT/
├── api/                 # .NET Core 8 API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Data/
│   └── appsettings.json
├── app/                 # Angular 16 Application
│   ├── src/
│   │   ├── app/
│   │   ├── assets/
│   │   └── environments/
│   └── angular.json
└── docker-compose.yml
```

## Features

- ✅ Patient Management (CRUD operations)
- ✅ Appointment Scheduling (CRUD operations)
- ✅ Repository Pattern for Data Access
- ✅ Service Layer with Validation
- ✅ Transactional write operations (controller-scoped)
- ✅ Comprehensive Exception Handling
- ✅ Structured Logging
- ✅ Hangfire background jobs and dashboard
- ✅ Responsive UI
- ✅ RESTful API with Swagger Documentation
- ✅ Entity Framework Core with SQL Server
- ✅ Docker Containerization

## Database

The SQL Server database is automatically initialized when the container starts. The API applies Entity Framework Core migrations on startup to create/update the schema.

- **Default Password**: Rxnt2024!Secure
- **Port**: 1433

⚠️ **Security Note**: Change the default password in production environments.

## API Endpoints

All API endpoints are documented via Swagger UI at http://localhost:5000/swagger

### Hangfire Dashboard

- URL: http://localhost:5000/hangfire/
- Exposed without authentication in Development inside the container.
- If you see 401 or an empty response, try a hard refresh or an incognito window.

## Technologies

### Frontend
- Angular 16
- TypeScript
- RxJS

### Backend
- .NET 8
- Entity Framework Core
- Repository Pattern
- Unit of Work Pattern
- Service Layer Architecture

### Database
- SQL Server 2022
- Entity Framework Core Migrations

### DevOps
- Docker & Docker Compose
- Nginx (for Angular app)

## Architecture

The application follows a layered architecture:

- Repository Pattern: Abstracts data access
- Service Layer: Business logic and validation
- Base Controller Pattern: Transaction management and error handling for write operations
- Exception Handling Middleware: Global error handling
- Logging: Structured logging throughout the application

For detailed architecture documentation, see [ARCHITECTURE.md](ARCHITECTURE.md).

## License

MIT
