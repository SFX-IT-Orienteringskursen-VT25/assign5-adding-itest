IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CalculationDb')
BEGIN
    CREATE DATABASE CalculationDb;
END
GO

USE CalculationDb;
GO

IF NOT EXISTS (
    SELECT * FROM sys.objects 
    WHERE object_id = OBJECT_ID(N'[dbo].[Calculations]') AND type in (N'U')
)
BEGIN
    CREATE TABLE Calculations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Operand1 FLOAT NOT NULL,
        Operand2 FLOAT NOT NULL,
        Operation NVARCHAR(10) NOT NULL,
        Result FLOAT NOT NULL
    );
END
GO
