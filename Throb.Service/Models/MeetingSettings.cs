using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Throb.Service.Models
{
    public class MeetingSettings
    {
        // 🟢 1. LocalRecording (لحل خطأ CS0117)
        [JsonPropertyName("local_recording")]
        public bool LocalRecording { get; set; } = false;

        // 🟢 2. AutoRecording (لحل خطأ CS0117)
        [JsonPropertyName("auto_recording")]
        public string AutoRecording { get; set; } = "none"; // القيمة يجب أن تكون "none" لتعطيل التسجيل

        // 🟢 3. HostVideo (قد يسبب خطأ CS0117 إذا لم يكن موجوداً)
        [JsonPropertyName("host_video")]
        public bool HostVideo { get; set; } = false;

        // 🟢 4. JoinBeforeHost
        [JsonPropertyName("join_before_host")]
        public bool JoinBeforeHost { get; set; } = false;

        // 🟢 5. Password (لحل خطأ CS8618 - بجعله قابلاً للقيم الفارغة أو بتعريفه في LiveSession.cs)
        [JsonPropertyName("password")]
        // إذا كنت تستخدم .NET 6+، قم بتعريفه كـ string? إذا كان في النموذج الأولي
        public string? Password { get; set; }// كلمة المرور
        [JsonPropertyName("participant_video")]
        public bool ParticipantVideo { get; set; } = false;
        // ... يمكن إضافة إعدادات أخرى
    }
}
