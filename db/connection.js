const sql = require('mssql');

// Configuration for different environments
const getDbConfig = () => {
  if (process.env.NODE_ENV === 'test') {
    const testConfig = JSON.parse(process.env.TEST_DB_CONFIG || '{}');
    return {
      server: testConfig.host || 'localhost',
      port: testConfig.port || 1433,
      database: testConfig.database || 'TestStorageDB',
      user: testConfig.user || 'sa',
      password: testConfig.password || 'YourPassword123!',
      options: {
        encrypt: false,
        trustServerCertificate: true,
        enableArithAbort: true
      }
    };
  } else {
    return {
      server: 'localhost',
      database: 'StorageDB',
      user: 'sa',
      password: 'YourPassword123!',
      port: 1433,
      options: {
        encrypt: false,
        trustServerCertificate: true,
        enableArithAbort: true
      }
    };
  }
};

let pool;

const getPool = async () => {
  if (!pool) {
    const config = getDbConfig();
    pool = new sql.ConnectionPool(config);
    
    try {
      await pool.connect();
      console.log(`Connected to ${config.database}`);
      
      if (process.env.NODE_ENV !== 'test') {
        await initializeDatabase();
      }
    } catch (error) {
      console.error('Database connection error:', error);
      throw error;
    }
  }
  return pool;
};

const initializeDatabase = async () => {
  try {
    const request = pool.request();
    
    // Create database if it doesn't exist
    await request.query(`
      IF NOT EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE name = 'StorageDB')
      CREATE DATABASE StorageDB
    `);
    
    // Switch to StorageDB
    await request.query('USE StorageDB');
    
    // Create table if it doesn't exist
    await request.query(`
      IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Storage' and xtype='U')
      CREATE TABLE Storage (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(255) NOT NULL UNIQUE,
        value NVARCHAR(MAX) NOT NULL,
        created_at DATETIME2 DEFAULT GETDATE(),
        updated_at DATETIME2 DEFAULT GETDATE()
      )
    `);
    
    console.log('Database initialized successfully');
  } catch (error) {
    console.error('Database initialization error:', error);
  }
};

module.exports = { getPool, sql, getDbConfig };