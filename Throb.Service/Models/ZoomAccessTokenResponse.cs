using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Throb.Service.Models
{
    public class ZoomAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")] // 💡 يفضل إضافة هذا لحزمة البيانات
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        // هذا الحقل سيتم تعبئته يدوياً في الخدمة لسهولة التحقق
        public DateTime ExpiryTime { get; set; }
    }
}
