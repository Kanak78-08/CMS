-- =============================================
-- Test Status Update Directly in Database
-- Run this to test if Status column can be updated
-- =============================================

USE ComplaintManagementDB;
GO

-- First, check current status values
PRINT '=== CURRENT STATUS VALUES ===';
SELECT 
    Id,
    Description,
    Status,
    ResolutionDateTime,
    ActionTaken
FROM Complaints
ORDER BY Id DESC;
GO

-- Test update - Change Status to 'Closed' for a specific complaint
-- Replace @Id with an actual complaint ID from your database
DECLARE @TestId INT = 1; -- Change this to an actual complaint ID
DECLARE @NewStatus NVARCHAR(50) = 'Closed'; -- Try: 'Pending', 'In Progress', or 'Closed'

PRINT '=== TESTING STATUS UPDATE ===';
PRINT 'Updating Complaint ID: ' + CAST(@TestId AS VARCHAR);
PRINT 'New Status: ' + @NewStatus;

-- Check if complaint exists
IF EXISTS (SELECT 1 FROM Complaints WHERE Id = @TestId)
BEGIN
    -- Get old status
    DECLARE @OldStatus NVARCHAR(50);
    SELECT @OldStatus = Status FROM Complaints WHERE Id = @TestId;
    PRINT 'Old Status: ' + @OldStatus;
    
    -- Perform update
    UPDATE Complaints 
    SET Status = @NewStatus
    WHERE Id = @TestId;
    
    -- Check if update was successful
    DECLARE @RowsAffected INT = @@ROWCOUNT;
    IF @RowsAffected > 0
    BEGIN
        PRINT 'SUCCESS: Status updated! Rows affected: ' + CAST(@RowsAffected AS VARCHAR);
        
        -- Verify the update
        SELECT 
            Id,
            Status AS 'New Status',
            Description
        FROM Complaints 
        WHERE Id = @TestId;
        
        PRINT 'Status update verified successfully!';
    END
    ELSE
    BEGIN
        PRINT 'ERROR: No rows were updated. Check if the ID exists.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: Complaint with ID ' + CAST(@TestId AS VARCHAR) + ' does not exist.';
    PRINT 'Please use a valid complaint ID from the list above.';
END
GO

-- Show all complaints with their current status
PRINT '=== ALL COMPLAINTS WITH STATUS ===';
SELECT 
    Id,
    SNo,
    Description,
    Status,
    AssignedTo,
    ResolutionDateTime,
    ActionTaken
FROM Complaints
ORDER BY Id DESC;
GO

PRINT '=== TEST COMPLETED ===';
PRINT 'If the update worked here but not in the application, check:';
PRINT '1. Application connection string';
PRINT '2. Application code UpdateComplaint method';
PRINT '3. Browser cache - try clearing cache or using incognito mode';
GO

