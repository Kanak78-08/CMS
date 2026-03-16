# Soft Delete Implementation - How It Works

## What is Soft Delete?

**Soft Delete** is a database pattern where records are **marked as deleted** instead of being **permanently removed** from the database. This means:

- ✅ Records stay in the database **forever**
- ✅ Records are **hidden** from the application UI
- ✅ You can **recover** deleted records if needed
- ✅ You have a **complete audit trail**

## How It Works in This System

### Database Changes

1. **Two new columns added to Complaints table:**
   - `IsDeleted` (bit): `0` = Active, `1` = Deleted
   - `DeletedDate` (datetime): Date/time when record was deleted

### When You Delete a Complaint

**Before (Hard Delete):**
```sql
DELETE FROM Complaints WHERE Id = 123
-- Record is permanently removed from database
```

**After (Soft Delete):**
```sql
UPDATE Complaints 
SET IsDeleted = 1, 
    DeletedDate = GETDATE()
WHERE Id = 123
-- Record stays in database, just marked as deleted
```

### When You View Complaints

**Before:**
```sql
SELECT * FROM Complaints
-- Shows all records
```

**After:**
```sql
SELECT * FROM Complaints WHERE IsDeleted = 0
-- Only shows active (non-deleted) records
```

## Implementation Details

### 1. Database Schema
- Run `AddSoftDeleteColumn.sql` to add the columns
- All existing records are automatically set to `IsDeleted = 0` (active)

### 2. Code Changes

#### Complaint Class (`Complaint.cs`)
- Added `IsDeleted` property (bool)
- Added `DeletedDate` property (DateTime?)

#### DatabaseHelper Methods

**GetAllComplaints()**
- Now filters: `WHERE IsDeleted = 0`
- Only returns active complaints

**DeleteComplaint()**
- Changed from `DELETE` to `UPDATE`
- Sets `IsDeleted = 1` and `DeletedDate = GETDATE()`
- Record remains in database

**GetComplaintById()**
- Now filters: `WHERE Id = @Id AND IsDeleted = 0`
- Won't return deleted complaints

**InsertComplaint()**
- Automatically sets `IsDeleted = 0` for new records

**GetNextSNo()**
- Only counts active records: `WHERE IsDeleted = 0`

## Benefits

1. **Data Preservation**: All complaints are kept forever for audit purposes
2. **Recovery**: Deleted complaints can be restored if needed
3. **History**: You can see when complaints were deleted
4. **Safety**: No accidental permanent data loss

## Viewing Deleted Records

If you want to see deleted complaints in SQL Server:

```sql
-- View all deleted complaints
SELECT * FROM Complaints WHERE IsDeleted = 1;

-- View all complaints (active + deleted)
SELECT * FROM Complaints;

-- Count deleted vs active
SELECT 
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END) AS Active,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS Deleted
FROM Complaints;
```

## Restoring Deleted Complaints

If you need to restore a deleted complaint:

```sql
UPDATE Complaints 
SET IsDeleted = 0, 
    DeletedDate = NULL
WHERE Id = 123;
```

## Setup Instructions

1. **Run the SQL script:**
   - Open SQL Server Management Studio
   - Open `AddSoftDeleteColumn.sql`
   - Execute it against your `ComplaintManagementDB` database

2. **Rebuild the application:**
   - Open Visual Studio
   - Build → Rebuild Solution

3. **Test:**
   - Delete a complaint from the application
   - Check in SSMS: `SELECT * FROM Complaints WHERE IsDeleted = 1`
   - You should see the deleted complaint with `IsDeleted = 1` and `DeletedDate` set

## Summary

- ✅ Complaints are **never permanently deleted**
- ✅ Deleted complaints are **hidden from the UI**
- ✅ All data is **preserved in the database**
- ✅ You can **restore** deleted complaints if needed
- ✅ Complete **audit trail** of deletions

