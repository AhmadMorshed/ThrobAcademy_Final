using System.Text.Json.Serialization;

// 💡 يجب وضع هذا النموذج في مجلد Throb.Service.Models
public class ZoomMeetingResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; } // مُعرّف الاجتماع في Zoom

    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    [JsonPropertyName("start_url")]
    public string StartUrl { get; set; } // رابط بدء المضيف (الأستاذ)

    [JsonPropertyName("join_url")]
    public string JoinUrl { get; set; } // رابط انضمام الطالب

    [JsonPropertyName("password")]
    public string Password { get; set; } // كلمة مرور الاجتماع

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    // ... يمكن إضافة حقول أخرى حسب الحاجة
}