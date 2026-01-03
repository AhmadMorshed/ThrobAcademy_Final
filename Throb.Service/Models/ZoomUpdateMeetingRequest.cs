using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Throb.Service.Models
{
    // 💻 داخل Service/Models/ZoomUpdateMeetingRequest.cs

    public class ZoomUpdateMeetingRequest
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; }

        [JsonPropertyName("start_time")]
        public string Start_time { get; set; } // يجب أن يكون بتنسيق ISO 8601 (yyyy-MM-ddTHH:mm:ssZ)

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("settings")]
        public MeetingSettings Settings { get; set; }
    }
}
