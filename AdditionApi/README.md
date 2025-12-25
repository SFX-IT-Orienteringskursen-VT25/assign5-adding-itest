# Assignment 4 - Adding Database to Addition API

This project extends Assignment 3 by adding MS SQL Server database persistence using Docker.

## Overview

The Addition API now uses MS SQL Server running in Docker as its persistence layer instead of in-memory storage. All key-value storage operations are performed against the database.

## Requirements Met

✅ The API uses MS SQL as the persistence layer  
✅ The MS SQL server is hosted in Docker  
✅ Methods for saving and retrieving data are done towards the MS SQL database

## Project Structure

```
AdditionApi/
├── AdditionApi.csproj       # Updated with Docker.DotNet and Microsoft.Data.SqlClient packages
├── Program.cs               # Modified to initialize Docker/Database and use MS SQL
├── Database.cs              # Database operations for key-value storage
├── DockerStarter.cs         # Docker container management for MS SQL Server
├── SqlCredentials.cs        # SQL Server credentials
├── appsettings.json
├── appsettings.Development.json
└── Properties/
    └── launchSettings.json
```

## Key Components

### Database.cs
- **Setup()**: Creates the database and storage table if they don't exist
- **SetValue(key, value)**: Inserts or updates a key-value pair
- **GetValue(key)**: Retrieves a value by key, returns null if not found
- **KeyExists(key)**: Checks if a key exists in the database

### DockerStarter.cs
- Automatically pulls the MS SQL Server 2022 Docker image
- Creates and starts a Docker container named "sqlserver"
- Configures port 1433 for SQL Server access
- Reuses existing container if already present

### API Endpoints

**GET /storage/{key}**
- Retrieves a value from the database by key
- Returns 200 OK with the value, or 404 Not Found if key doesn't exist

**PUT /storage/{key}**
- Stores or updates a value in the database by key
- Returns 201 Created if new key was created, or 200 OK if updated

## Prerequisites

- .NET 9.0 SDK
- Docker Desktop installed and running
- Port 1433 available for SQL Server

**Note for ARM Macs (M1/M2/M3):** MS SQL Server 2022 does not support ARM architecture natively. If you're on an ARM Mac:
- Use Docker Desktop (not Rancher Desktop) with Rosetta emulation enabled
- Or test on an x86_64 system (Windows, Linux x86_64, or Intel Mac)
- Alternative: Temporarily use Azure SQL Edge by changing the image in `DockerStarter.cs` to `mcr.microsoft.com/azure-sql-edge:latest`

## How to Run

1. Ensure Docker Desktop is running
2. Navigate to the AdditionApi directory
3. Run the application:
   ```bash
   dotnet run
   ```

The application will:
- Start MS SQL Server in a Docker container
- Create the database and table structure
- Start the web API on the configured port

## Testing the API

Using curl or any HTTP client:

```bash
# Store a value
curl -X PUT https://localhost:{port}/storage/mykey \
  -H "Content-Type: application/json" \
  -d '{"value": "myvalue"}'

# Retrieve a value
curl https://localhost:{port}/storage/mykey
```

## Database Details

- **Database Name**: AdditionApiDatabase
- **Table Name**: Storage
- **Schema**:
  - Key (VARCHAR(255), Primary Key)
  - Value (VARCHAR(MAX))

## Changes from Assignment 3

1. Added NuGet packages:
   - Docker.DotNet (3.125.15)
   - Microsoft.Data.SqlClient (6.1.1)

2. Removed in-memory Dictionary storage
3. Added Docker container management
4. Implemented SQL-based storage operations
5. Modified API endpoints to use database methods

## SQL Server Credentials

- **Username**: sa
- **Password**: password123!
- **Server**: localhost,1433

⚠️ Note: These credentials are for development only. Use secure credentials in production.
