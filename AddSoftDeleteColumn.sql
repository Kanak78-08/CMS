-- =============================================
-- Add Soft Delete Column to Complaints Table
-- This implements "soft delete" - records are marked as deleted
-- but remain in the database forever
-- =============================================

USE ComplaintManagementDB;
GO

-- Step 1: Add IsDeleted column (bit type: 0 = not deleted, 1 = deleted)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Complaints]
    ADD [IsDeleted] [bit] NOT NULL DEFAULT (0);
    
    PRINT 'IsDeleted column added successfully.';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists.';
END
GO

-- Step 2: Add DeletedDate column (to track when record was deleted)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'DeletedDate')
BEGIN
    ALTER TABLE [dbo].[Complaints]
    ADD [DeletedDate] [datetime] NULL;
    
    PRINT 'DeletedDate column added successfully.';
END
ELSE
BEGIN
    PRINT 'DeletedDate column already exists.';
END
GO

-- Step 3: Update existing records to ensure IsDeleted is 0 (not deleted)
-- Only update if IsDeleted column exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE [dbo].[Complaints]
    SET [IsDeleted] = 0
    WHERE [IsDeleted] IS NULL;
    
    PRINT 'Existing records marked as not deleted.';
END
ELSE
BEGIN
    PRINT 'IsDeleted column does not exist. Skipping update.';
END
GO

-- Step 4: Verify the columns were added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Complaints' 
  AND (COLUMN_NAME = 'IsDeleted' OR COLUMN_NAME = 'DeletedDate')
ORDER BY COLUMN_NAME;
GO

-- Step 5: Show current status (only if IsDeleted column exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    SELECT 
        COUNT(*) AS TotalComplaints,
        SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END) AS ActiveComplaints,
        SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedComplaints
    FROM Complaints;
END
ELSE
BEGIN
    SELECT 
        COUNT(*) AS TotalComplaints,
        COUNT(*) AS ActiveComplaints,
        0 AS DeletedComplaints
    FROM Complaints;
    PRINT 'IsDeleted column does not exist. Showing total count only.';
END
GO

PRINT '========================================';
PRINT 'Soft Delete Setup Complete!';
PRINT '========================================';
PRINT 'How it works:';
PRINT '1. When you delete a complaint, IsDeleted is set to 1';
PRINT '2. DeletedDate is set to current date/time';
PRINT '3. Record remains in database forever';
PRINT '4. Application only shows records where IsDeleted = 0';
PRINT '========================================';
GO

