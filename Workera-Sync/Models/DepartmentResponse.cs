using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Workera_Sync.Models
{
    public class DepartmentResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
        [JsonPropertyName("data")]
        public List<DepartmentData> Data { get; set; }
    }
}
