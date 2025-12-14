const { GenericContainer, Wait } = require('testcontainers');
const mssql = require('mssql');

let container;
let dbPool;

module.exports = async () => {
  // Start MS SQL Server in Docker for testing
  console.log('Starting MS SQL Server container for integration tests...');
  
  container = await new GenericContainer('mcr.microsoft.com/mssql/server:2019-latest')
    .withEnvironment({
      ACCEPT_EULA: 'Y',
      SA_PASSWORD: 'YourPassword123!'
    })
    .withExposedPorts(1433)
    .withWaitStrategy(Wait.forLogMessage('SQL Server is now ready for client connections'))
    .start();

  const host = container.getHost();
  const port = container.getMappedPort(1433);

  // Build connection config
  const config = {
    server: host,
    port: port,
    database: 'master',
    user: 'sa',
    password: 'YourPassword123!',
    options: {
      encrypt: false,
      trustServerCertificate: true,
      enableArithAbort: true
    }
  };

  // Create connection pool
  dbPool = new mssql.ConnectionPool(config);
  await dbPool.connect();
  console.log('Connected to test database container');

  // Create test database and table
  await initializeTestDatabase();
  
  // Set environment variable for the test database connection
  process.env.TEST_DB_CONFIG = JSON.stringify({
    host: host,
    port: port,
    user: 'sa',
    password: 'YourPassword123!',
    database: 'TestStorageDB'
  });

  global.__TEST_CONTAINER__ = container;
  global.__TEST_DB_POOL__ = dbPool;
};

async function initializeTestDatabase() {
  try {
    // Create test database
    await dbPool.request().query(`
      IF NOT EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE name = 'TestStorageDB')
      CREATE DATABASE TestStorageDB
    `);
    
    // Switch to test database
    await dbPool.request().query('USE TestStorageDB');
    
    // Create storage table
    await dbPool.request().query(`
      IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Storage' and xtype='U')
      CREATE TABLE Storage (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(255) NOT NULL UNIQUE,
        value NVARCHAR(MAX) NOT NULL,
        created_at DATETIME2 DEFAULT GETDATE(),
        updated_at DATETIME2 DEFAULT GETDATE()
      )
    `);
    
    console.log('Test database initialized');
  } catch (error) {
    console.error('Error initializing test database:', error);
    throw error;
  }
}

// Cleanup function called after all tests
async function cleanup() {
  if (dbPool) {
    await dbPool.close();
    console.log('Test database connection closed');
  }
  
  if (container) {
    await container.stop();
    console.log('Test container stopped');
  }
}

// Export cleanup for use in tests
module.exports.cleanup = cleanup;