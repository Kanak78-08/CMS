-- =============================================
-- Remove ImagePath Column from Complaints Table
-- =============================================

USE ComplaintManagementDB;
GO

-- Check if ImagePath column exists before removing
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'Complaints' AND COLUMN_NAME = 'ImagePath')
BEGIN
    -- Remove the ImagePath column
    ALTER TABLE [dbo].[Complaints]
    DROP COLUMN [ImagePath];
    
    PRINT 'ImagePath column removed successfully from Complaints table.';
END
ELSE
BEGIN
    PRINT 'ImagePath column does not exist in Complaints table.';
END
GO

-- Verify the column has been removed
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Complaints'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Script completed successfully!';
GO

