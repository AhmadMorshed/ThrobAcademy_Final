using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Repository.Repositories;
using Throb.Service.Interfaces;

namespace Throb.Service.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public QuestionService(IQuestionRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<Course>> GetAllCoursesForSelectionAsync() => await _repository.GetAllCoursesAsync();

      
        public async Task<List<Question>> GetQuestionsByCourseAsync(int courseId) => await _repository.GetQuestionsByCourseIdAsync(courseId);

        public async Task<string> GenerateAndStoreQuestionsAsync(string transcript, string type, int courseId)
        {
            if (string.IsNullOrWhiteSpace(transcript) || courseId <= 0) return "❌ بيانات غير مكتملة";

            var aiResponse = await GenerateQuestionsFromAI(transcript, type);
            if (aiResponse.StartsWith("❌")) return aiResponse;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<AiDetailedResponse>(aiResponse, options);

                if (result?.Questions == null || !result.Questions.Any())
                    return "❌ لم يتم العثور على أسئلة في رد الذكاء الاصطناعي.";

                int savedCount = 0;
                foreach (var qObj in result.Questions)
                {
                    if (string.IsNullOrWhiteSpace(qObj.Text)) continue;

                    var question = new Question
                    {
                        QuestionText = qObj.Text.Trim(),
                        QuestionType = type.ToLower(),
                        CourseId = courseId,
                        Transcript = transcript,
                        CorrectAnswer = qObj.Answer ?? "",
                        Difficulty = "Medium",
                        CreatedAt = DateTime.Now,
                        IsManual = false,
                        
                        Options = qObj.Options?.Select(optText => new QuestionOption
                        {
                            OptionText = optText.Trim()
                        }).ToList() ?? new List<QuestionOption>()
                    };

                    await _repository.AddAsync(question);
                    savedCount++;
                }

                return savedCount > 0 ? $"✅ تم حفظ {savedCount} أسئلة مع خياراتها بنجاح." : "❌ فشل الحفظ.";
            }
            catch (Exception ex)
            {
                return $"❌ خطأ في معالجة البيانات: {ex.Message}";
            }
        }

        private async Task<string> GenerateQuestionsFromAI(string transcript, string type)
        {
            var apiKey = _configuration["OpenRouterApiKey"];
            var client = _httpClientFactory.CreateClient();


            string structure = type.Equals("MCQ", StringComparison.OrdinalIgnoreCase)
                ? "{\"questions\": [{\"text\": \"السؤال؟\", \"options\": [\"خيار 1\", \"خيار 2\", \"خيار 3\", \"خيار 4\"], \"answer\": \"النص المطابق للإجابة الصحيحة\"}]}"
                : "{\"questions\": [{\"text\": \"السؤال؟\", \"answer\": \"True/False\"}]}";

            string prompt = $"Extract 10 {type} questions from this text. Language: Arabic. " +
                            $"Return ONLY a JSON object with this structure: {structure} " +
                            $"\n\nText: {transcript}";

            var requestBody = new
            {
                model = "meta-llama/llama-3.3-70b-instruct",
                messages = new[]
                {
                    new { role = "system", content = "You are a JSON expert. Never include text outside the JSON." },
                    new { role = "user", content = prompt }
                },
                response_format = new { type = "json_object" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

        public Task<List<Question>> GetAllQuestionsAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            
            return await _repository.GetByIdAsync(id);
          
        }
        public class AiDetailedResponse { public List<AiQuestionItem> Questions { get; set; } }
        public class AiQuestionItem
        {
            public string Text { get; set; }
            public List<string> Options { get; set; } 
            public string Answer { get; set; }
        }
    }
}