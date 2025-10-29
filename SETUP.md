# RXNT Clinic Manager - Setup Guide

## Prerequisites

Before setting up the application, ensure you have the following installed:

- Docker Desktop (latest version)
- Docker Compose (comes with Docker Desktop)
- Git (optional, for version control)

## Architecture

The application consists of three main services:

1. **Frontend (Angular 16)**: Served on port 4200
2. **Backend (.NET Core 8 API)**: Served on port 5000
3. **Database (SQL Server 2022)**: Running on port 1433

## Quick Start with Docker

### Step 1: Clone or Download the Project

If you have the project in a repository:
```bash
git clone <repository-url>
cd RXNT
```

### Step 2: Start All Services

From the root directory (where docker-compose.yml is located):

```bash
docker-compose up --build
```

This command will:
- Build the Angular application
- Build the .NET Core API
- Pull the SQL Server image
- Create and start all three containers
- Set up the database automatically

### Step 3: Access the Application

Once all containers are running:

- **Frontend**: http://localhost:4200
- **API**: http://localhost:5000
- **Swagger API Documentation**: http://localhost:5000/swagger
- **Database**: localhost:1433

### Step 4: Stop the Services

To stop all containers:
```bash
docker-compose down
```

To stop and remove volumes (clears database data):
```bash
docker-compose down -v
```

## Local Development Setup

If you prefer to run the applications locally without Docker:

### Backend (.NET Core 8 API)

1. **Navigate to the API directory:**
   ```bash
   cd api
   ```

2. **Update the connection string** in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=RXNTClinicManager;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
   }
   ```

3. **Restore dependencies and run:**
   ```bash
   dotnet restore
   dotnet run
   ```

   The API will be available at http://localhost:5000

### Frontend (Angular 16)

1. **Navigate to the app directory:**
   ```bash
   cd app
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Start the development server:**
   ```bash
   npm start
   ```

   The application will be available at http://localhost:4200

## Database Configuration

### SQL Server Credentials

- **Server**: sqlserver (in Docker) or localhost
- **Username**: sa
- **Password**: Rxnt2024!Secure
- **Database**: RXNTClinicManager

⚠️ **Security Warning**: Change the default password in production!

### Connecting to the Database

You can connect to the SQL Server database using:
- SQL Server Management Studio (SSMS)
- Azure Data Studio
- Visual Studio
- Any SQL Server client

**Connection Details:**
- Server: `localhost,1433`
- Username: `sa`
- Password: `Rxnt2024!Secure`

## Troubleshooting

### Port Already in Use

If you encounter port conflicts:

1. **Stop the service using the port:**
   ```bash
   # Windows PowerShell
   netstat -ano | findstr :4200
   taskkill /PID <PID> /F
   ```

2. **Or change the port** in `docker-compose.yml`

### Database Connection Issues

1. **Check if SQL Server is running:**
   ```bash
   docker ps
   ```

2. **Check SQL Server logs:**
   ```bash
   docker logs rxnt-sqlserver
   ```

3. **Verify connection string** in `appsettings.json`

### Build Errors

1. **Clear Docker cache:**
   ```bash
   docker-compose down -v
   docker system prune -a
   docker-compose up --build
   ```

## Project Structure

```
RXNT/
├── api/                      # .NET Core 8 API
│   ├── Controllers/          # API Controllers
│   ├── Data/                 # DbContext and Database
│   ├── Models/               # Entity Models
│   ├── Services/             # Business Logic
│   ├── Dockerfile
│   └── RXNT.API.csproj
├── app/                      # Angular 16 Application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/   # Reusable Components
│   │   │   ├── pages/        # Page Components
│   │   │   ├── services/     # API Services
│   │   │   └── models/       # TypeScript Models
│   │   └── styles.css
│   ├── Dockerfile
│   └── nginx.conf
└── docker-compose.yml        # Docker orchestration
```

## Features

- ✅ Patient Management (CRUD operations)
- ✅ Appointment Scheduling (CRUD operations)
- ✅ Dashboard with Statistics
- ✅ Responsive UI
- ✅ RESTful API with Swagger Documentation
- ✅ Entity Framework Core with SQL Server
- ✅ Docker Containerization

## API Endpoints

### Patients
- `GET /api/patients` - Get all patients
- `GET /api/patients/{id}` - Get patient by ID
- `POST /api/patients` - Create new patient
- `PUT /api/patients/{id}` - Update patient
- `DELETE /api/patients/{id}` - Delete patient

### Appointments
- `GET /api/appointments` - Get all appointments
- `GET /api/appointments/{id}` - Get appointment by ID
- `POST /api/appointments` - Create new appointment
- `PUT /api/appointments/{id}` - Update appointment
- `DELETE /api/appointments/{id}` - Delete appointment

## Support

For issues or questions, please refer to the main README.md file.
