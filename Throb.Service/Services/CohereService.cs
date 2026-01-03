using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // أضف هذا السطر

namespace Throb.Services
{
    public class CohereService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; // إزالة القيمة المباشرة من هنا
        private readonly string _endpoint = "https://api.cohere.ai/v1/chat";

        // التعديل: إضافة IConfiguration للمشيد
        public CohereService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // جلب المفتاح من appsettings.json
            _apiKey = configuration["Cohere:ApiKey"];
        }

        public async Task<string> GenerateExamQuestions(string prompt)
        {
            // التحقق من أن المفتاح تم قراءته بنجاح
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Cohere ApiKey is missing. Please check appsettings.json");
            }

            var requestBody = new
            {
                model = "command-r7b-12-2024",
                message = prompt,
                temperature = 0.1
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_endpoint, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("text").GetString() ?? "[]";
                }

                throw new HttpRequestException($"Cohere API Error: {response.StatusCode} - {jsonResponse}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Service Error: {ex.Message}");
            }
        }
    }
}