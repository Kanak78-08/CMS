using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ComplaintManagementSystem
{
    public class DatabaseHelper
    {
        private static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ComplaintDBConnection"].ConnectionString;
            }
        }

        // Get all complaints from database (only active, non-deleted records)
        public static List<Complaint> GetAllComplaints()
        {
            List<Complaint> complaints = new List<Complaint>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Only get records where IsDeleted = 0 (not deleted)
                string query = @"SELECT Id, SNo, Description, UserDetails, LogDate, AssignedTo, ResolutionDateTime, 
                                ActionTaken, Status, CreatedDate, IsDeleted, DeletedDate 
                                FROM Complaints 
                                WHERE IsDeleted = 0 
                                ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            complaints.Add(new Complaint
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                SNo = Convert.ToInt32(reader["SNo"]),
                                Description = reader["Description"].ToString(),
                                UserDetails = reader["UserDetails"]?.ToString() ?? "",
                                LogDate = Convert.ToDateTime(reader["LogDate"]),
                                AssignedTo = reader["AssignedTo"].ToString(),
                                ResolutionDateTime = reader["ResolutionDateTime"] != DBNull.Value 
                                    ? (DateTime?)Convert.ToDateTime(reader["ResolutionDateTime"]) 
                                    : null,
                                ActionTaken = reader["ActionTaken"]?.ToString() ?? "",
                                Status = (reader["Status"]?.ToString() ?? "").Trim(),
                                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                                DeletedDate = reader["DeletedDate"] != DBNull.Value 
                                    ? (DateTime?)Convert.ToDateTime(reader["DeletedDate"]) 
                                    : null
                            });
                        }
                    }
                }
            }

            // Re-number SNo based on order
            int sNo = 1;
            foreach (var complaint in complaints.OrderBy(c => c.Id))
            {
                complaint.SNo = sNo++;
            }

            return complaints;
        }

        // Insert a new complaint
        public static int InsertComplaint(Complaint complaint)
        {
            // Get the next SNo (only from active records)
            int nextSNo = GetNextSNo();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"INSERT INTO Complaints 
                    (SNo, Description, UserDetails, LogDate, AssignedTo, ResolutionDateTime, 
                     ActionTaken, Status, CreatedDate, IsDeleted, DeletedDate)
                    VALUES 
                    (@SNo, @Description, @UserDetails, @LogDate, @AssignedTo, @ResolutionDateTime, 
                     @ActionTaken, @Status, @CreatedDate, @IsDeleted, @DeletedDate);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SNo", nextSNo);
                    cmd.Parameters.AddWithValue("@Description", complaint.Description);
                    cmd.Parameters.AddWithValue("@UserDetails", (object)complaint.UserDetails ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LogDate", complaint.LogDate);
                    cmd.Parameters.AddWithValue("@AssignedTo", complaint.AssignedTo);
                    cmd.Parameters.AddWithValue("@ResolutionDateTime", 
                        (object)complaint.ResolutionDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionTaken", (object)complaint.ActionTaken ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", complaint.Status);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@IsDeleted", false); // New complaints are always active
                    cmd.Parameters.AddWithValue("@DeletedDate", DBNull.Value); // Not deleted yet

                    conn.Open();
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return newId;
                }
            }
        }

        // Update an existing complaint
        public static bool UpdateComplaint(Complaint complaint)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"UPDATE Complaints SET
                    SNo = @SNo,
                    Description = @Description,
                    UserDetails = @UserDetails,
                    LogDate = @LogDate,
                    AssignedTo = @AssignedTo,
                    ResolutionDateTime = @ResolutionDateTime,
                    ActionTaken = @ActionTaken,
                    Status = @Status
                    WHERE Id = @Id AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", complaint.Id);
                    cmd.Parameters.AddWithValue("@SNo", complaint.SNo);
                    cmd.Parameters.AddWithValue("@Description", complaint.Description);
                    cmd.Parameters.AddWithValue("@UserDetails", (object)complaint.UserDetails ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LogDate", complaint.LogDate);
                    cmd.Parameters.AddWithValue("@AssignedTo", complaint.AssignedTo);
                    cmd.Parameters.AddWithValue("@ResolutionDateTime", 
                        (object)complaint.ResolutionDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionTaken", (object)complaint.ActionTaken ?? DBNull.Value);
                    
                    // Ensure status is not null or empty - use the exact value from complaint object
                    string statusValue = complaint.Status ?? "";
                    statusValue = statusValue.Trim();
                    
                    // If somehow empty, default to Pending (should not happen with validation)
                    if (string.IsNullOrEmpty(statusValue))
                    {
                        statusValue = "Pending";
                    }
                    
                    // Use SqlParameter with explicit type to ensure proper database update
                    SqlParameter statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 50);
                    statusParam.Value = statusValue;
                    cmd.Parameters.Add(statusParam);
                    
                    // Debug: Log the status value being saved
                    System.Diagnostics.Debug.WriteLine("=== DATABASE UPDATE DEBUG ===");
                    System.Diagnostics.Debug.WriteLine("Complaint ID: " + complaint.Id);
                    System.Diagnostics.Debug.WriteLine("Status Value: '" + statusValue + "'");
                    System.Diagnostics.Debug.WriteLine("Status Length: " + statusValue.Length);
                    System.Diagnostics.Debug.WriteLine("SQL Query: " + query);

                    conn.Open();
                    
                    // Log all parameter values before execution
                    System.Diagnostics.Debug.WriteLine("=== PARAMETER VALUES ===");
                    foreach (SqlParameter param in cmd.Parameters)
                    {
                        System.Diagnostics.Debug.WriteLine(param.ParameterName + " = '" + (param.Value?.ToString() ?? "NULL") + "'");
                    }
                    
                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    // Log the result
                    System.Diagnostics.Debug.WriteLine("=== UPDATE RESULT ===");
                    System.Diagnostics.Debug.WriteLine("Rows Affected: " + rowsAffected);
                    System.Diagnostics.Debug.WriteLine("Status value saved: '" + statusValue + "'");
                    
                    // Verify the update by reading back
                    if (rowsAffected > 0)
                    {
                        string verifyQuery = "SELECT Status FROM Complaints WHERE Id = @Id";
                        using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, conn))
                        {
                            verifyCmd.Parameters.AddWithValue("@Id", complaint.Id);
                            object result = verifyCmd.ExecuteScalar();
                            string actualStatus = result != null ? result.ToString().Trim() : "NULL";
                            System.Diagnostics.Debug.WriteLine("=== VERIFICATION ===");
                            System.Diagnostics.Debug.WriteLine("Expected Status: '" + statusValue + "'");
                            System.Diagnostics.Debug.WriteLine("Actual Status in DB: '" + actualStatus + "'");
                            System.Diagnostics.Debug.WriteLine("Match: " + (actualStatus == statusValue ? "YES" : "NO"));
                            
                            if (actualStatus != statusValue)
                            {
                                System.Diagnostics.Debug.WriteLine("WARNING: Status mismatch! Update may have failed.");
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: No rows were affected by the update!");
                    }
                    
                    return rowsAffected > 0;
                }
            }
        }

        // Soft delete a complaint (marks as deleted but keeps in database)
        public static bool DeleteComplaint(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Instead of DELETE, we UPDATE IsDeleted = 1 and set DeletedDate
                // This way the record stays in database forever
                string query = @"UPDATE Complaints 
                                SET IsDeleted = 1, 
                                    DeletedDate = GETDATE()
                                WHERE Id = @Id AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    System.Diagnostics.Debug.WriteLine("Soft Delete: Complaint ID " + id + " marked as deleted. Rows affected: " + rowsAffected);
                    
                    return rowsAffected > 0;
                }
            }
        }

        // Get a single complaint by ID (only if not deleted)
        public static Complaint GetComplaintById(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Only get if not deleted (IsDeleted = 0)
                string query = @"SELECT Id, SNo, Description, UserDetails, LogDate, AssignedTo, ResolutionDateTime, 
                                ActionTaken, Status, CreatedDate, IsDeleted, DeletedDate 
                                FROM Complaints 
                                WHERE Id = @Id AND IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Complaint
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                SNo = Convert.ToInt32(reader["SNo"]),
                                Description = reader["Description"].ToString(),
                                UserDetails = reader["UserDetails"]?.ToString() ?? "",
                                LogDate = Convert.ToDateTime(reader["LogDate"]),
                                AssignedTo = reader["AssignedTo"].ToString(),
                                ResolutionDateTime = reader["ResolutionDateTime"] != DBNull.Value 
                                    ? (DateTime?)Convert.ToDateTime(reader["ResolutionDateTime"]) 
                                    : null,
                                ActionTaken = reader["ActionTaken"]?.ToString() ?? "",
                                Status = (reader["Status"]?.ToString() ?? "").Trim(),
                                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                                DeletedDate = reader["DeletedDate"] != DBNull.Value 
                                    ? (DateTime?)Convert.ToDateTime(reader["DeletedDate"]) 
                                    : null
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Get the next SNo (only from active, non-deleted records)
        private static int GetNextSNo()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                // Only count active records (IsDeleted = 0)
                string query = "SELECT ISNULL(MAX(SNo), 0) + 1 FROM Complaints WHERE IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 1;
                }
            }
        }

        // Re-number all SNo values (only for active, non-deleted records)
        public static void RenumberSNo()
        {
            var complaints = GetAllComplaints(); // This already filters out deleted records
            int sNo = 1;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                foreach (var complaint in complaints.OrderBy(c => c.Id))
                {
                    string query = "UPDATE Complaints SET SNo = @SNo WHERE Id = @Id AND IsDeleted = 0";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SNo", sNo);
                        cmd.Parameters.AddWithValue("@Id", complaint.Id);
                        cmd.ExecuteNonQuery();
                    }
                    sNo++;
                }
            }
        }
    }
}

