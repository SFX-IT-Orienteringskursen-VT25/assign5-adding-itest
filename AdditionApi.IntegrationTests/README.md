# Integration Tests for AdditionApi

## Overview
This project contains comprehensive integration tests for the AdditionApi storage service using:
- **xUnit** - Testing framework
- **Testcontainers** - For spinning up a real SQL Server database in Docker
- **Microsoft.AspNetCore.Mvc.Testing** - For in-memory API testing

## Project Structure

```
AdditionApi.IntegrationTests/
├── AdditionApi.IntegrationTests.csproj  # Test project dependencies
├── AdditionApiFactory.cs                 # Custom WebApplicationFactory for tests
└── StorageApiTests.cs                    # Integration test suite
```

## Test Coverage

The integration tests cover the following scenarios:

### PUT /storage/{key} Tests
- ✅ Creating a new key returns 201 Created
- ✅ Updating an existing key returns 200 OK
- ✅ Storing empty values
- ✅ Handling special characters in keys
- ✅ Storing long values
- ❌ Missing request body returns 400 Bad Request
- ❌ Invalid JSON returns 400 Bad Request
- ❌ Missing Value property returns 400 Bad Request

### GET /storage/{key} Tests
- ✅ Retrieving existing key returns 200 OK with value
- ❌ Retrieving non-existent key returns 404 Not Found
- ❌ Empty key returns 404 Not Found
- ✅ Retrieving updated values
- ✅ Handling special characters in keys

### Integration Workflow Tests
- ✅ Complete CRUD workflow (Create, Read, Update, Read)
- ✅ Multiple keys can be stored independently

### Negative Tests
- ❌ Invalid HTTP method returns 405 Method Not Allowed

## Requirements Compliance

✅ **Integration tested**: Tests invoke the API using its HTTP endpoints  
✅ **Real database**: Uses SQL Server container via Testcontainers  
✅ **Negative tests**: Includes error scenarios (404, 400, 405)  
✅ **One command execution**: Run with `dotnet test`  
✅ **Automatic setup**: Database container initialized automatically by tests

## Running the Tests

### Prerequisites
1. Docker Desktop must be running
2. .NET 9.0 SDK installed

### Running Tests

```bash
# From the repository root
dotnet test AdditionApi.IntegrationTests/AdditionApi.IntegrationTests.csproj

# With verbose output
dotnet test AdditionApi.IntegrationTests/AdditionApi.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

### Cleaning Build Artifacts

```bash
# Clean all build outputs and temporary files
dotnet clean
```

This will remove all `bin/` and `obj/` directories from all projects in the solution.

## Docker Configuration

No additional configuration needed. Testcontainers will automatically detect Docker Desktop.

## Troubleshooting

### Tests fail with "Docker is not running"
- Ensure Docker Desktop is running
- Check Docker is accessible: `docker ps`

### Tests fail with container errors
- Try cleaning up old containers: `docker ps -a | grep testcontainers | awk '{print $1}' | xargs docker rm -f`
- Restart Docker
- Ensure you have enough disk space and memory allocated to Docker

### Port conflicts
- Testcontainers uses random ports, but if you have issues, stop any SQL Server instances running on port 1433

## Implementation Details

### AdditionApiFactory
- Extends `WebApplicationFactory<Program>` for testing
- Uses `IAsyncLifetime` for container lifecycle management  
- Automatically starts SQL Server container before tests
- Configures the API to use the test database
- Cleans up containers after tests complete

### Test Database
- Each test run uses a fresh SQL Server 2022 container
- Database and table are created automatically
- Connection string is injected into the API for testing
- Container is disposed after all tests complete

## CI/CD Integration

To run these tests in CI/CD:

```yaml
# Example GitHub Actions workflow
- name: Run Integration Tests
  run: dotnet test AdditionApi.IntegrationTests/AdditionApi.IntegrationTests.csproj
  env:
    DOCKER_HOST: unix:///var/run/docker.sock
```

## Test Execution Time

- Initial run: ~30-60 seconds (includes Docker image pull)
- Subsequent runs: ~10-20 seconds

## Future Enhancements

- Add tests for concurrent access
- Add performance/load tests
- Add tests for database failures/resilience
- Integrate with code coverage tools
