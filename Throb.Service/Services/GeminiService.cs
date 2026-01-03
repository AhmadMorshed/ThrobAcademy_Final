using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Throb.Service.Interfaces.GeminiAI;

namespace Throb.Service.Implementations.GeminiAI
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiApiKey"];
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
        }

        public async Task<List<int>> GetSmartSelectionAsync(string userPrompt, string questionsJson)
        {
            // تعليمات صارمة ومختصرة
            var systemInstruction = $@"
    Task: Act as a JSON filter.
    Input: A list of questions (JSON) and a user request.
    Rules:
    1. Analyze the user request to identify: Difficulty (سهل/Easy, متوسط/Medium, صعب/Hard) and Count (number of questions).
    2. Return ONLY a plain JSON array of the 'id's that match the criteria.
    3. If the user asks for a specific number (e.g. '3 questions'), return exactly 3 IDs.
    4. Return ONLY the array [1, 2, 3]. NO text, NO markdown.

    Data: {questionsJson}";

            var promptPayload = new
            {
                contents = new[] {
            new { parts = new[] { new { text = $"User Request: {userPrompt}\n\nResponse must be a plain JSON array of IDs based on the data provided in system instructions." } } }
        },
                generationConfig = new
                {
                    temperature = 0.1, // لضمان استجابة محددة وغير إبداعية
                    responseMimeType = "application/json" // إجبار الموديل على الرد بتنسيق JSON
                }
            };

            try
            {
                var jsonPayload = JsonSerializer.Serialize(promptPayload);
                var response = await _httpClient.PostAsync(_apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode) return new List<int>();

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                var aiRawText = doc.RootElement.GetProperty("candidates")[0]
                                               .GetProperty("content")
                                               .GetProperty("parts")[0]
                                               .GetProperty("text").GetString();

                // تنظيف الاستجابة بشكل أقوى
                string cleanedText = aiRawText.Replace("```json", "").Replace("```", "").Trim();

                // استخراج المصفوفة بأمان
                if (cleanedText.StartsWith("[") && cleanedText.EndsWith("]"))
                {
                    return JsonSerializer.Deserialize<List<int>>(cleanedText) ?? new List<int>();
                }

                // محاولة أيرة في حال وجود نص زائد
                var match = Regex.Match(cleanedText, @"\[(\d+,\s*)*\d+\]");
                return match.Success ? JsonSerializer.Deserialize<List<int>>(match.Value) : new List<int>();
            }
            catch { return new List<int>(); }
        }
    }
    }
