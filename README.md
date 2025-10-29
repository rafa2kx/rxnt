# RXNT Clinic Manager

A full-stack application for managing clinic operations, built with Angular 16, .NET Core 8 API, and SQL Server, all containerized with Docker.

## Architecture

- **Frontend**: Angular 16 with modern UI/UX
- **Backend**: .NET Core 8 Web API
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
docker-compose up --build

# 2. Wait for all containers to be ready (about 1-2 minutes)

# 3. Access the application
# Frontend: http://localhost:4200
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
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
- ✅ Unit of Work Pattern for Transactions
- ✅ Service Layer with Validation
- ✅ Transactional Database Operations (Automatic Rollback on Failures)
- ✅ Comprehensive Exception Handling
- ✅ Structured Logging
- ✅ Dashboard with Statistics
- ✅ Responsive UI
- ✅ RESTful API with Swagger Documentation
- ✅ Entity Framework Core with SQL Server
- ✅ Docker Containerization

## Database

The SQL Server database is automatically initialized when the container starts. The database schema is created using Entity Framework migrations.

- **Default Password**: Rxnt2024!Secure
- **Port**: 1433

⚠️ **Security Note**: Change the default password in production environments.

## API Endpoints

All API endpoints are documented via Swagger UI at http://localhost:5000/swagger

## Technologies

### Frontend
- Angular 16
- TypeScript
- RxJS

### Backend
- .NET Core 8
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

The application follows a layered architecture with Base Controller pattern:

- **Base Controller Pattern**: Provides transaction management and error handling for all controllers
- **Repository Pattern**: Abstracts data access logic (for read operations)
- **Service Layer**: Contains business logic and validation
- **Transactional Operations**: All write operations in controllers automatically run within transactions with rollback on failures
- **Exception Handling**: Centralized error handling in BaseController and global middleware
- **Logging**: Structured logging throughout the application

For detailed architecture documentation, see [ARCHITECTURE.md](ARCHITECTURE.md).

## License

MIT
