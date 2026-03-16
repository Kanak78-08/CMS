using System;

namespace ComplaintManagementSystem
{
    [Serializable]
    public class Complaint
    {
        public int Id { get; set; }
        public int SNo { get; set; }
        public string Description { get; set; }
        public string UserDetails { get; set; }
        public DateTime LogDate { get; set; } // Now contains both date and time
        public string AssignedTo { get; set; }
        public DateTime? ResolutionDateTime { get; set; }
        public string ActionTaken { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; } // Soft delete flag: false = active, true = deleted
        public DateTime? DeletedDate { get; set; } // Date when record was soft deleted
    }
}

