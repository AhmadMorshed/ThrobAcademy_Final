using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Throb.Service.Models; // 💡 تأكد من أن هذا هو النطاق الذي يحتوي على ZoomAccessTokenResponse
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace Throb.Service.Services
{
   
    // 2. الكلاس
    public class ZoomAuthService : IZoomAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        // 🟢 تصحيح: تم تغيير static إلى حقل مثيل عادي (Instance Field)
        private ZoomAccessTokenResponse _cachedToken = null;

        public ZoomAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _config = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // 1. التحقق من الرمز المخزن (مع التحقق من أن الرمز لا يزال صالحاً لمدة 5 دقائق على الأقل)
            if (_cachedToken != null && _cachedToken.ExpiryTime > DateTime.UtcNow.AddMinutes(5))
            {
                return _cachedToken.AccessToken;
            }

            // 2. إذا كان الرمز غير صالح، قم بطلب رمز جديد (S2S OAuth Flow)

            // جلب المفاتيح من appsettings.json
            var clientId = _config["ZoomSettings:ClientId"];
            var clientSecret = _config["ZoomSettings:ClientSecret"];
            var accountId = _config["ZoomSettings:AccountId"];

            // ⚠️ التحقق من وجود المفاتيح قبل إرسال الطلب (أفضل ممارسة)
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(accountId))
            {
                throw new ApplicationException("Zoom API credentials (ClientId, ClientSecret, AccountId) are missing in appsettings.json.");
            }

            // إعداد الطلب (URL: https://zoom.us/oauth/token?grant_type=account_credentials&account_id=...)
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://zoom.us/oauth/token?grant_type=account_credentials&account_id={accountId}");

            // إضافة رأس التوثيق الأساسي (Base64 Encoded ClientID:ClientSecret)
            // ... في ZoomAuthService.cs
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))
            );
            // ...

            var response = await _httpClient.SendAsync(request);

            // 💡 معالجة الأخطاء غير الناجحة (مثل 401 Unauthorized)
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to retrieve Zoom Access Token. Status: {response.StatusCode}. Content: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<ZoomAccessTokenResponse>(content);

            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                // 3. تخزين الرمز الجديد وحساب وقت الانتهاء
                tokenResponse.ExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                _cachedToken = tokenResponse;
                return _cachedToken.AccessToken;
            }

            throw new ApplicationException("Failed to retrieve Zoom Access Token. Response was null or empty.");
        }
    }
}