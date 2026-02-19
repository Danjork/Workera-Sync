using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workera_Sync.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public string Identification { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public DateTime AttendanceData { get; set; }
        public int AttendanceType { get; set; }
        public string? Address { get; set; }
        public DateTime SyncDate { get; set; } = DateTime.Now;

    }
}
