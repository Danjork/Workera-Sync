using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Workera_Sync.Models
{
    public class EmployeeData
    {
        [JsonPropertyName("code")]
        public int code { get; set; }

        [JsonPropertyName("deviceCode")]
        public int deviceCode { get; set; }

        [JsonPropertyName("identification")]
        public string identification { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; 
        }
        [JsonPropertyName("lastName")]
        public string lastName { get; set; }

        [JsonPropertyName("branchOffice")]
        public string branchOffice { get; set; }

        [JsonPropertyName("department")]
        public string department { get; set; }

        [JsonPropertyName("employeeStatus")]
        public string  employeeStatus { get; set; }

        [JsonPropertyName("companyIdentification")]
        public string  companyIdentification { get; set; }

        [JsonPropertyName("companyName")]
        public string companyName { get; set; }
    }
}
