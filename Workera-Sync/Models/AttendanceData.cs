using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Workera_Sync.Models
{
    public class AttendanceData

    {
        //El registro de asistencia que une al trabajador con su marcación

        [JsonPropertyName("employee")]
        public EmployeeData Employee { get; set; } // Ahora es un objeto 

        [JsonPropertyName("attendanceDate")]
        public DateTime AttendanceDate { get; set; } // Nombre exacto del JSON 

        [JsonPropertyName("attendanceType")]
        public int AttendanceType { get; set; } // Cambiado a int 

        [JsonPropertyName("origin")]
        public string origin { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        public int code { get; set; }

        [JsonPropertyName("identification")]
        public string identification { get; set; }

        [JsonPropertyName("checksum")]
        public string checksum { get; set; }

        [JsonPropertyName("originCode")]
        public string originCode { get; set; }
    }
}
