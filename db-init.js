import sql from "mssql";
import { config } from "./db.js";

const masterConfig = {
    ...config,
};

async function initDb() {
    try {
        const pool = await sql.connect(masterConfig);

        await pool.request().query(`
      IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '${process.env.DB_NAME}')
      BEGIN
        CREATE DATABASE ${process.env.DB_NAME};
      END
    `);

        console.log(`✔ Database '${process.env.DB_NAME}' is ready.`);

        const appPool = await sql.connect(config);

        await appPool.request().query(`
      IF NOT EXISTS (
        SELECT * FROM sysobjects WHERE name='buckets' AND xtype='U'
      )
      CREATE TABLE dbo.buckets (
        [key] NVARCHAR(256) NOT NULL PRIMARY KEY,
        [sum] DECIMAL(18,4) NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
      );
    `);

        await appPool.request().query(`
      IF NOT EXISTS (
        SELECT * FROM sysobjects WHERE name='bucket_entries' AND xtype='U'
      )
      CREATE TABLE dbo.bucket_entries (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(256) NOT NULL,
        [value] DECIMAL(18,4) NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_bucket_entries_key 
          FOREIGN KEY ([key]) REFERENCES dbo.buckets([key]) 
          ON DELETE CASCADE
      );
    `);

        console.log("✔ All required tables created.");

    } catch (err) {
        console.error("DB Init Error:", err);
    } finally {
        sql.close();
    }
}

initDb();
