-- Merge LogDate and CallReceived columns into a single LogDateTime column
-- This script safely checks for column existence before operations

USE ComplaintManagementDB;
GO

-- Check current table structure
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Complaints' 
AND COLUMN_NAME IN ('LogDate', 'CallReceived', 'LogDateTime')
ORDER BY COLUMN_NAME;
GO

-- Step 1: Check if LogDateTime already exists, if not create it
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'LogDateTime')
BEGIN
    ALTER TABLE Complaints
    ADD LogDateTime DATETIME NULL;
    PRINT 'LogDateTime column added.';
END
ELSE
BEGIN
    PRINT 'LogDateTime column already exists.';
END
GO

-- Step 2: Populate LogDateTime by combining LogDate and CallReceived
-- Check if CallReceived column exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'CallReceived')
BEGIN
    -- CallReceived exists - combine with LogDate
    UPDATE Complaints
    SET LogDateTime = CASE 
        WHEN CallReceived IS NOT NULL AND CallReceived != '' AND LEN(LTRIM(RTRIM(CallReceived))) > 0
        THEN CAST(CAST(LogDate AS DATETIME) AS DATE) + CAST(CallReceived AS TIME)
        WHEN LogDate IS NOT NULL
        THEN CAST(LogDate AS DATETIME)
        ELSE GETDATE()
    END
    WHERE LogDateTime IS NULL;
    PRINT 'LogDateTime populated from LogDate and CallReceived.';
END
ELSE
BEGIN
    -- CallReceived doesn't exist - just use LogDate
    UPDATE Complaints
    SET LogDateTime = CASE 
        WHEN LogDate IS NOT NULL
        THEN CAST(LogDate AS DATETIME)
        ELSE GETDATE()
    END
    WHERE LogDateTime IS NULL;
    PRINT 'LogDateTime populated from LogDate only (CallReceived column not found).';
END
GO

-- Step 3: Set any remaining NULLs to current date/time
UPDATE Complaints
SET LogDateTime = GETDATE()
WHERE LogDateTime IS NULL;
GO

-- Step 4: Make LogDateTime NOT NULL
ALTER TABLE Complaints
ALTER COLUMN LogDateTime DATETIME NOT NULL;
GO
PRINT 'LogDateTime set to NOT NULL.';
GO

-- Step 5: Drop old LogDate column if it exists and is different from LogDateTime
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'LogDate')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'LogDateTime')
BEGIN
    -- Only drop if LogDateTime doesn't exist (meaning we haven't renamed yet)
    ALTER TABLE Complaints
    DROP COLUMN LogDate;
    PRINT 'Old LogDate column dropped.';
END
ELSE IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'LogDate')
         AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                     WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'LogDateTime')
BEGIN
    -- Both exist - drop the old LogDate
    ALTER TABLE Complaints
    DROP COLUMN LogDate;
    PRINT 'Old LogDate column dropped.';
END
ELSE
BEGIN
    PRINT 'LogDate column not found or already processed.';
END
GO

-- Step 6: Drop CallReceived column if it exists
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'CallReceived')
BEGIN
    ALTER TABLE Complaints
    DROP COLUMN CallReceived;
    PRINT 'CallReceived column dropped.';
END
ELSE
BEGIN
    PRINT 'CallReceived column not found (may have been dropped already).';
END
GO

-- Step 7: Rename LogDateTime to LogDate if LogDate doesn't exist
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'LogDateTime')
   AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                   WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'LogDate')
BEGIN
    EXEC sp_rename 'Complaints.LogDateTime', 'LogDate', 'COLUMN';
    PRINT 'LogDateTime renamed to LogDate.';
END
ELSE
BEGIN
    PRINT 'Rename skipped - LogDate already exists or LogDateTime not found.';
END
GO

-- Verify the final structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Complaints' 
AND COLUMN_NAME IN ('LogDate', 'CallReceived', 'LogDateTime')
ORDER BY COLUMN_NAME;
GO

-- Show sample data
SELECT TOP 5 Id, LogDate, Status FROM Complaints;
GO

PRINT 'Migration completed successfully!';
GO

