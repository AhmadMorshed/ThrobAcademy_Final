using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Throb.Service.Models;

public interface IZoomAuthService
{
    Task<string> GetAccessTokenAsync();
}

public class ZoomAuthService : IZoomAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    // 🔒 لتخزين الرمز ووقته مؤقتاً
    private static ZoomAccessTokenResponse _cachedToken = null;

    public ZoomAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _config = configuration;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // 1. التحقق من الرمز المخزن
        // نطلب رمزا جديدا إذا كان الرمز غير موجود أو سيصبح غير صالح خلال 5 دقائق
        if (_cachedToken != null && _cachedToken.ExpiryTime > DateTime.UtcNow.AddMinutes(5))
        {
            return _cachedToken.AccessToken;
        }

        // 2. إذا كان الرمز غير صالح، قم بطلب رمز جديد

        // جلب المفاتيح من appsettings.json
        var clientId = _config["ZoomSettings:ClientId"];
        var clientSecret = _config["ZoomSettings:ClientSecret"];
        var accountId = _config["ZoomSettings:AccountId"];

        // إعداد الطلب
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://zoom.us/oauth/token?grant_type=account_credentials&account_id={accountId}");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))
        );

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<ZoomAccessTokenResponse>(content);

        if (tokenResponse != null)
        {
            // 3. تخزين الرمز الجديد وحساب وقت الانتهاء
            tokenResponse.ExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            _cachedToken = tokenResponse;
            return _cachedToken.AccessToken;
        }

        throw new ApplicationException("Failed to retrieve Zoom Access Token.");
    }
}