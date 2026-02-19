using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Workera_Sync.Models
{
    public class RequestInfo
    {
        //metadatos de la consulta
        [JsonPropertyName("companyName")]
        public string companyName { get; set; }

        [JsonPropertyName("companyIdentification")]
        public string companyIdentification { get; set; }

        [JsonPropertyName("companyNickname")]
        public string companyNickname { get; set; }

        [JsonPropertyName("userEmail")]
        public string userEmail { get; set; }
    }
}
