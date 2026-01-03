using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Throb.Service.Models
{
    // 💡 داخل Throb.Service.Models

    public class ZoomParticipant
    {
        [JsonPropertyName("id")]
        public string UserId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("user_email")]
        public string Email { get; set; }

        // 🟢 الوقت الذي قضاه الطالب في الاجتماع (بالدقائق)
        [JsonPropertyName("duration")]
        public int DurationMinutes { get; set; }

        // ... حقول أخرى مثل وقت الانضمام والمغادرة
    }

    public class ZoomMeetingParticipantsResponse
    {
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("total_records")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("participants")]
        public List<ZoomParticipant> Participants { get; set; }
    }
}
