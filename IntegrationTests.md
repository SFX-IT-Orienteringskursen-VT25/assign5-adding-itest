# Integration Tests

## Overview

Integration tests verify the API endpoints work correctly with a real SQL Server database using [Testcontainers](https://testcontainers.com/). A Docker container is spun up automatically for each test run.

## Prerequisites

- **Docker** must be running on your machine
- Node.js 18+

## Test Scripts

| Command | Description |
|---------|-------------|
| `npm test` | Runs unit tests first, then integration tests |
| `npm run test:unit` | Runs only unit tests (fast, no Docker) |
| `npm run test:integration` | Runs only integration tests |
| `npm run test:watch` | Watch mode for development |

## How It Works

1. **Container Setup** — Before tests run, a SQL Server container starts automatically
2. **Dynamic Configuration** — Environment variables are set from the container's connection details
3. **Database Initialization** — Tables are created in the test database
4. **Test Execution** — API endpoints are tested against the real database
5. **Cleanup** — Container is stopped after tests complete

## Test File Structure

```
tests/
├── api.integration.test.js   # Integration tests (uses Testcontainers)
├── storage.test.js           # Unit tests
└── utils.test.js             # Unit tests
```

## Key Implementation Details

### Test Setup (`beforeAll`)

```javascript
container = await new MSSQLServerContainer("mcr.microsoft.com/mssql/server:2022-latest")
    .acceptLicense()
    .start();

process.env.DB_HOST = container.getHost();
process.env.DB_PORT = String(container.getPort());
process.env.DB_USER = container.getUsername();
process.env.DB_PASSWORD = container.getPassword();
process.env.DB_NAME = "appdb_test";
```

### Test Cleanup (`afterAll`)

```javascript
const { resetPool } = await import("../db.js");
await resetPool();
await container.stop();
```

## Tested Endpoints

| Endpoint | Method | Tests |
|----------|--------|-------|
| `/db-version` | GET | Returns SQL Server version |
| `/storage` | GET | Lists all buckets |
| `/storage/:key` | GET | Gets bucket by key, 404 if not found |
| `/storage/:key` | POST | Creates/updates bucket with values |

## AI Usage for this assignment

- [x] started the process with asking questions with chatgpt how I can initially setup the integration tests, then adjust/refactor the code myself and tested with unit tests and check manually those changes are okay.

- [x] wrote the test as previous with vitest but when running the test the containers were failing so solved them with the help with chatgpt
