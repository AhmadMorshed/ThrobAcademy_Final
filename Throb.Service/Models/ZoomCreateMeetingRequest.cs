using System.Text.Json.Serialization;
using Throb.Service.Models;

// 💡 يجب وضع هذا النموذج في مجلد Throb.Service.Models
public class ZoomCreateMeetingRequest
{
    [JsonPropertyName("settings")]
    public MeetingSettings Settings { get; set; } = new MeetingSettings();
    // دائماً نستخدم "me" لإنشاء الاجتماع على حساب الماستر (يمكن أن يكون MasterEmail)
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "me";

    [JsonPropertyName("topic")]
    public string Topic { get; set; } // عنوان الاجتماع

    [JsonPropertyName("type")]
    public int Type { get; set; } = 2; // 2 = Scheduled Meeting

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; } // تنسيق ISO 8601 (YYYY-MM-DDTHH:mm:ssZ)

    [JsonPropertyName("duration")]
    public int Duration { get; set; } // المدة بالدقائق

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = "Asia/Damascus";




   
}