using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Workera_Sync.Models
{
    public class AttendanceResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("totalPages")]
        public int totalPages { get; set; }

        [JsonPropertyName("pageResult")]
        public int pageResult { get; set; }

        [JsonPropertyName("totalResult")]
        public int totalResult { get; set; }

        [JsonPropertyName("requestInfo")]
        public RequestInfo info { get; set; }


        [JsonPropertyName("data")]
        public List<AttendanceData> data { get; set; }
    }
}
