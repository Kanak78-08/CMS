-- =============================================
-- Complaint Management System Database Setup
-- SQL Server Database Script
-- =============================================

-- Step 1: Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ComplaintManagementDB')
BEGIN
    CREATE DATABASE ComplaintManagementDB;
    PRINT 'Database ComplaintManagementDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database ComplaintManagementDB already exists.';
END
GO

-- Step 2: Use the Database
USE ComplaintManagementDB;
GO

-- Step 3: Create Complaints Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Complaints]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Complaints](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SNo] [int] NOT NULL,
        [Description] [nvarchar](500) NOT NULL,
        [UserDetails] [nvarchar](200) NULL,
        [LogDate] [datetime] NOT NULL,
        [AssignedTo] [nvarchar](100) NOT NULL,
        [ResolutionDateTime] [datetime] NULL,
        [ActionTaken] [nvarchar](1000) NULL,
        [Status] [nvarchar](50) NOT NULL,
        [CallReceived] [nvarchar](50) NULL,
        [CreatedDate] [datetime] NOT NULL DEFAULT (getdate()),
        CONSTRAINT [PK_Complaints] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table Complaints created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Complaints already exists.';
END
GO

-- Step 4: Verify Table Creation
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Complaints'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Database setup completed successfully!';
PRINT 'You can now use this database in your application.';
GO

