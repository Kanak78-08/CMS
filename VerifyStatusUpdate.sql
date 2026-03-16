-- =============================================
-- Verify Status Column Update
-- This script helps verify that Status updates are working correctly
-- =============================================

USE ComplaintManagementDB;
GO

-- Check the Status column definition
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'Status';
GO

-- Check current Status values in the database
SELECT 
    Id,
    Description,
    Status,
    ResolutionDateTime,
    ActionTaken,
    LogDate
FROM Complaints
ORDER BY Id DESC;
GO

-- Test update query (example - replace @Id and @Status with actual values)
-- UPDATE Complaints 
-- SET Status = 'Closed'
-- WHERE Id = 1;
-- GO

-- Verify the update worked
-- SELECT Id, Status FROM Complaints WHERE Id = 1;
-- GO

PRINT 'Status column verification completed.';
PRINT 'If Status values are not updating, check:';
PRINT '1. The Status column allows NULL values (should be NOT NULL)';
PRINT '2. There are no triggers preventing updates';
PRINT '3. The application is using the correct connection string';
GO

