-- =============================================
-- QUICK STATUS UPDATE TEST
-- Run this to quickly test if status update works
-- =============================================

USE ComplaintManagementDB;
GO

-- Show current status of all complaints
SELECT 
    Id,
    Status,
    Description,
    AssignedTo
FROM Complaints
ORDER BY Id DESC;
GO

-- Test update - CHANGE THE ID BELOW TO YOUR COMPLAINT ID
DECLARE @TestId INT = 1; -- ⚠️ CHANGE THIS TO YOUR ACTUAL COMPLAINT ID
DECLARE @NewStatus NVARCHAR(50) = 'Closed'; -- Try: 'Pending', 'In Progress', or 'Closed'

PRINT 'Testing Status Update...';
PRINT 'Complaint ID: ' + CAST(@TestId AS VARCHAR);
PRINT 'New Status: ' + @NewStatus;

-- Get old status
DECLARE @OldStatus NVARCHAR(50);
SELECT @OldStatus = Status FROM Complaints WHERE Id = @TestId;

IF @OldStatus IS NULL
BEGIN
    PRINT 'ERROR: Complaint ID ' + CAST(@TestId AS VARCHAR) + ' does not exist!';
    PRINT 'Please check the ID from the list above.';
END
ELSE
BEGIN
    PRINT 'Old Status: ' + @OldStatus;
    
    -- Update
    UPDATE Complaints 
    SET Status = @NewStatus
    WHERE Id = @TestId;
    
    -- Check result
    IF @@ROWCOUNT > 0
    BEGIN
        -- Verify
        DECLARE @ActualStatus NVARCHAR(50);
        SELECT @ActualStatus = Status FROM Complaints WHERE Id = @TestId;
        
        PRINT 'Update Result: SUCCESS';
        PRINT 'New Status in DB: ' + @ActualStatus;
        
        IF @ActualStatus = @NewStatus
        BEGIN
            PRINT '✓ Status updated correctly!';
        END
        ELSE
        BEGIN
            PRINT '✗ ERROR: Status mismatch!';
            PRINT 'Expected: ' + @NewStatus;
            PRINT 'Got: ' + @ActualStatus;
        END
    END
    ELSE
    BEGIN
        PRINT 'ERROR: Update failed - no rows affected!';
    END
END
GO

-- Show updated status
SELECT 
    Id,
    Status,
    Description
FROM Complaints
ORDER BY Id DESC;
GO

