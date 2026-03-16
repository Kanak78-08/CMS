-- Merge LogDate and CallReceived columns into a single LogDateTime column
-- This script will:
-- 1. Add a new LogDateTime column (DATETIME)
-- 2. Populate it by combining existing LogDate and CallReceived
-- 3. Drop the old LogDate and CallReceived columns
-- 4. Rename LogDateTime to LogDate

USE ComplaintManagementDB;
GO

-- Step 1: Add new LogDateTime column
ALTER TABLE Complaints
ADD LogDateTime DATETIME NULL;
GO

-- Step 2: Populate LogDateTime by combining LogDate and CallReceived
-- Parse CallReceived time (HH:mm format) and combine with LogDate
-- Convert DATE to DATETIME first, then add TIME
UPDATE Complaints
SET LogDateTime = CASE 
    WHEN CallReceived IS NOT NULL AND CallReceived != '' AND LEN(LTRIM(RTRIM(CallReceived))) > 0
    THEN CAST(CAST(LogDate AS DATETIME) AS DATE) + CAST(CallReceived AS TIME)
    WHEN LogDate IS NOT NULL
    THEN CAST(LogDate AS DATETIME)
    ELSE GETDATE() -- Default to current date/time if both are NULL
END;
GO

-- Step 3: Check for any NULL values before making column NOT NULL
-- If there are NULLs, set them to current date/time
UPDATE Complaints
SET LogDateTime = GETDATE()
WHERE LogDateTime IS NULL;
GO

-- Step 4: Make LogDateTime NOT NULL (after data migration)
ALTER TABLE Complaints
ALTER COLUMN LogDateTime DATETIME NOT NULL;
GO

-- Step 5: Drop old columns
ALTER TABLE Complaints
DROP COLUMN LogDate;
GO

ALTER TABLE Complaints
DROP COLUMN CallReceived;
GO

-- Step 6: Rename LogDateTime to LogDate
EXEC sp_rename 'Complaints.LogDateTime', 'LogDate', 'COLUMN';
GO

-- Verify the changes
SELECT TOP 5 Id, LogDate, Status FROM Complaints;
GO

