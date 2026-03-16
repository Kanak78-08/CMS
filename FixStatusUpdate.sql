-- =============================================
-- FIX STATUS UPDATE ISSUE
-- This script will help identify and fix the status update problem
-- =============================================

USE ComplaintManagementDB;
GO

-- Step 1: Check current Status column definition
PRINT '=== STEP 1: Checking Status Column Definition ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'Status';
GO

-- Step 2: Check if there are any triggers on the Complaints table
PRINT '=== STEP 2: Checking for Triggers ===';
SELECT 
    t.name AS TriggerName,
    OBJECT_NAME(t.parent_id) AS TableName
FROM sys.triggers t
WHERE OBJECT_NAME(t.parent_id) = 'Complaints';
GO

-- Step 3: Check current Status values
PRINT '=== STEP 3: Current Status Values ===';
SELECT 
    Id,
    Description,
    Status,
    LEN(Status) AS StatusLength,
    ASCII(LEFT(Status, 1)) AS FirstCharASCII,
    AssignedTo
FROM Complaints
ORDER BY Id DESC;
GO

-- Step 4: Test direct update (Replace @Id with actual complaint ID)
PRINT '=== STEP 4: Testing Direct Update ===';
DECLARE @TestId INT = 1; -- CHANGE THIS TO YOUR COMPLAINT ID
DECLARE @NewStatus NVARCHAR(50) = 'Closed'; -- Try: 'Pending', 'In Progress', or 'Closed'

-- Get old status
DECLARE @OldStatus NVARCHAR(50);
SELECT @OldStatus = Status FROM Complaints WHERE Id = @TestId;

PRINT 'Testing Update for Complaint ID: ' + CAST(@TestId AS VARCHAR);
PRINT 'Old Status: [' + @OldStatus + ']';
PRINT 'New Status: [' + @NewStatus + ']';

-- Perform update
UPDATE Complaints 
SET Status = LTRIM(RTRIM(@NewStatus))  -- Trim to remove any spaces
WHERE Id = @TestId;

-- Check result
DECLARE @RowsAffected INT = @@ROWCOUNT;
PRINT 'Rows Affected: ' + CAST(@RowsAffected AS VARCHAR);

-- Verify
DECLARE @ActualStatus NVARCHAR(50);
SELECT @ActualStatus = Status FROM Complaints WHERE Id = @TestId;
PRINT 'Actual Status in DB: [' + @ActualStatus + ']';

IF @RowsAffected > 0 AND @ActualStatus = @NewStatus
BEGIN
    PRINT 'SUCCESS: Status updated correctly!';
END
ELSE
BEGIN
    PRINT 'ERROR: Status update failed or value mismatch!';
    PRINT 'Expected: [' + @NewStatus + ']';
    PRINT 'Actual: [' + @ActualStatus + ']';
END
GO

-- Step 5: Check for any CHECK constraints
PRINT '=== STEP 5: Checking for CHECK Constraints ===';
SELECT 
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
INNER JOIN sys.objects o ON cc.parent_object_id = o.object_id
WHERE o.name = 'Complaints';
GO

-- Step 6: Show all complaints with their status
PRINT '=== STEP 6: All Complaints with Status ===';
SELECT 
    Id,
    SNo,
    LEFT(Description, 30) AS Description,
    Status,
    AssignedTo,
    CASE 
        WHEN Status = 'Pending' THEN '✓'
        WHEN Status = 'In Progress' THEN '✓'
        WHEN Status = 'Closed' THEN '✓'
        ELSE '✗ INVALID'
    END AS StatusValid
FROM Complaints
ORDER BY Id DESC;
GO

PRINT '=== DIAGNOSTIC COMPLETE ===';
PRINT 'If direct update worked but application does not:';
PRINT '1. Check application connection string';
PRINT '2. Check if Status parameter is being passed correctly';
PRINT '3. Check browser cache';
PRINT '4. Check application logs for errors';
GO

