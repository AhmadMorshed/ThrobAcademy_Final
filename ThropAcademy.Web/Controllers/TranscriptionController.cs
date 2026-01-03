using Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Service.Interfaces;
using Throb.Service.Interfaces.GeminiAI;
using Throb.Services;
using ThropAcademy.Web.Models;


namespace Throb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TranscriptionController : Controller
    {
        private readonly string _deepgramApiKey;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IQuestionService _questionService;
        private readonly IExamRequestService _examRequestService;
        private readonly ICourseService _courseService;
        private readonly IGeminiService _geminiService;
        private readonly CohereService _cohereService;
        private readonly ThrobDbContext _context;

        public TranscriptionController(IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IQuestionService questionService,
            IExamRequestService examRequestService,
            ICourseService courseService,
            IGeminiService geminiService,
            CohereService cohereService,
            ThrobDbContext context)
        {
            _deepgramApiKey = configuration.GetValue<string>("DeepgramApiKey") ?? "";
            _httpClientFactory = httpClientFactory;
            _questionService = questionService;
            _examRequestService = examRequestService;
            _courseService = courseService;
            _geminiService = geminiService;
            _cohereService = cohereService;
            _context = context;
        }

        #region Helpers
        private void SafeDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch { /* تجاهل الخطأ مؤقتاً */ }
        }
        #endregion

        #region Operations: Upload & Transcription
        [HttpGet]
        public async Task<IActionResult> UploadMedia(int? courseId)
        {
            ViewBag.Courses = await _questionService.GetAllCoursesForSelectionAsync();
            ViewBag.SelectedCourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadMedia(IFormFile mediaFile, int courseId)
        {
            string tempFilePath = "";
            try
            {
                if (mediaFile == null || mediaFile.Length == 0)
                {
                    ViewBag.ErrorMessage = "❌ يرجى اختيار ملف فيديو أو صوت صالح.";
                    ViewBag.Courses = await _questionService.GetAllCoursesForSelectionAsync();
                    return View();
                }

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                tempFilePath = Path.Combine(uploadsDir, Guid.NewGuid() + Path.GetExtension(mediaFile.FileName));

                using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await mediaFile.CopyToAsync(stream);
                }

                string transcript = await ProcessMediaViaApiAsync(tempFilePath);

                if (string.IsNullOrEmpty(transcript) || transcript.StartsWith("❌"))
                {
                    ViewBag.ErrorMessage = transcript ?? "❌ فشل الذكاء الاصطناعي في معالجة الملف.";
                    ViewBag.Courses = await _questionService.GetAllCoursesForSelectionAsync();
                    return View();
                }

                ViewBag.Transcription = transcript;
                ViewBag.CourseId = courseId;
                return View("Result");
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = $"خطأ تقني: {ex.Message}" });
            }
            finally
            {
                SafeDeleteFile(tempFilePath);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQuestions(string transcript, string type, int courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transcript))
                {
                    TempData["ErrorMessage"] = "❌ النص المستخرج فارغ، يرجى إعادة المحاولة.";
                    return RedirectToAction("UploadMedia", new { courseId = courseId });
                }

                var result = await _questionService.GenerateAndStoreQuestionsAsync(transcript, type, courseId);
                return RedirectToAction("QuestionBank", new { courseId = courseId });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"@@@ Error in GenerateQuestions: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = $"فشل توليد الأسئلة: {ex.Message}" });
            }
        }
        #endregion

        #region AI Smart Selection (Gemini)
        [HttpPost]
        public async Task<IActionResult> SmartSelectQuestions(int courseId, string userPrompt)
        {
            if (courseId <= 0 || string.IsNullOrWhiteSpace(userPrompt))
            {
                TempData["ErrorMessage"] = "يرجى كتابة ما تطلبه من الذكاء الاصطناعي (مثلاً: اختر 5 أسئلة).";
                return RedirectToAction("CreateExam", new { courseId = courseId });
            }

            try
            {
                var allQuestions = await _questionService.GetQuestionsByCourseAsync(courseId);

                // 1. تبسيط البيانات لتقليل الـ Tokens (تحسين الأداء والتكلفة)
                var simplifiedQuestions = allQuestions.Select(q => new
                {
                    id = q.QuestionId,
                    diff = q.Difficulty,
                    txt = q.QuestionText
                }).ToList();

                var questionsJson = JsonSerializer.Serialize(simplifiedQuestions);

                // 2. استدعاء خدمة Gemini
                var selectedIds = await _geminiService.GetSmartSelectionAsync(userPrompt, questionsJson);

                // 3. التحقق من النتائج (تجنب الهلوسة البرمجية)
                if (selectedIds == null || !selectedIds.Any())
                {
                    TempData["ErrorMessage"] = "عذراً، لم أستطع فهم الطلب أو لم أجد أسئلة مناسبة. حاول تبسيط العبارة.";
                    return RedirectToAction("CreateExam", new { courseId = courseId });
                }

                // 4. تأمين الـ IDs (للتأكد أن Gemini لم يرسل IDs غير موجودة)
                var finalValidIds = selectedIds.Intersect(allQuestions.Select(q => q.QuestionId)).ToList();

                // 5. التوجيه الأفضل: نعود لصفحة الإنشاء مع تفعيل الـ Selection
                // نرسل الـ IDs عبر الـ TempData أو الـ ViewBag
                TempData["SelectedIds"] = JsonSerializer.Serialize(finalValidIds);

                return RedirectToAction("CreateExam", new { courseId = courseId });
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ (يمكنك استخدام Logger هنا)
                TempData["ErrorMessage"] = "حدث خطأ تقني أثناء الاتصال بالذكاء الاصطناعي. يرجى المحاولة لاحقاً.";
                return RedirectToAction("CreateExam", new { courseId = courseId });
            }
        }
        #endregion

        #region Management: Question Bank & Exams
        [HttpGet]
        public async Task<IActionResult> QuestionBank(int? courseId)
        {
            int targetId = courseId ?? 0;
            // تأكد أن الخدمة داخل GetQuestionsByCourseAsync تستخدم .Include(o => o.Options)
            var questions = await _questionService.GetQuestionsByCourseAsync(targetId);
            var course = _courseService.GetById(targetId);

            ViewBag.CourseId = targetId;
            ViewBag.CourseName = course?.Name ?? "كورس غير محدد";

            return View(questions);
        }

        [HttpGet]
        public async Task<IActionResult> CreateExam(int courseId)
        {
            var questions = await _questionService.GetQuestionsByCourseAsync(courseId);
            ViewBag.Questions = questions;
            ViewBag.CourseId = courseId;

            if (TempData["SelectedIds"] != null)
            {
                ViewBag.SelectedIds = JsonSerializer.Deserialize<List<int>>(TempData["SelectedIds"].ToString());
            }

            return View(new ExamRequestModel { CourseId = courseId });
        }
        [HttpPost]
        public async Task<IActionResult> CreateExam(ExamRequestModel model, List<int> SelectedQuestionIds)
        {
            try
            {
                // التأكد من اختيار أسئلة
                if (SelectedQuestionIds == null || !SelectedQuestionIds.Any())
                {
                    TempData["ErrorMessage"] = "يرجى اختيار سؤال واحد على الأقل.";
                    return RedirectToAction("CreateExam", new { courseId = model.CourseId });
                }

                // جلب الأسئلة المختارة فقط
                var allQuestions = await _questionService.GetQuestionsByCourseAsync(model.CourseId);
                var selectedQuestions = allQuestions
                    .Where(q => SelectedQuestionIds.Contains(q.QuestionId))
                    .ToList();

                // تعبئة بيانات الموديل للمعاينة
                model.Questions = selectedQuestions;
                model.NumberOfQuestions = selectedQuestions.Count;

                // ملاحظة: قيمة model.ExamType ستأتي تلقائياً من الواجهة 
                // لأننا استخدمنا asp-for="ExamType" في الراديو بوتون

                return View("ExamPreview", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إعداد المعاينة: " + ex.Message;
                return RedirectToAction("CreateExam", new { courseId = model.CourseId });
            }
        }
        #endregion

        #region Technical Logic (FFmpeg & API)
        private async Task<string> ProcessMediaViaApiAsync(string videoPath)
        {
            string audioPath = Path.Combine(Path.GetDirectoryName(videoPath)!, Guid.NewGuid() + ".mp3");
            var ffmpegArgs = $"-y -i \"{videoPath}\" -vn -ar 16000 -ac 1 -ab 64k -f mp3 \"{audioPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var p = Process.Start(psi)) { p?.WaitForExit(); }

            if (!System.IO.File.Exists(audioPath))
                return "❌ فشل FFmpeg في تجهيز ملف الصوت.";

            string result = await SendToDeepgramApiAsync(audioPath);
            SafeDeleteFile(audioPath);
            return result;
        }

        private async Task<string> SendToDeepgramApiAsync(string filePath)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(20);

                var url = "https://api.deepgram.com/v1/listen?detect_language=true&smart_format=true&punctuate=true";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Token {_deepgramApiKey}");

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var content = new ByteArrayContent(fileBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
                request.Content = content;

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return $"❌ API error: {response.StatusCode} - {errorDetails}";
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                return doc.RootElement
                    .GetProperty("results")
                    .GetProperty("channels")[0]
                    .GetProperty("alternatives")[0]
                    .GetProperty("transcript").GetString() ?? "❌ لم يتم استخراج نص.";
            }
            catch (Exception ex)
            {
                return $"❌ خطأ اتصال: {ex.Message}";
            }
        }
        #endregion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(ExamRequestModel model, Dictionary<int, string> Answers)
        {
            // 1. حساب النتيجة
            int score = 0;
            int totalQuestions = Answers?.Count ?? 0;

            if (Answers != null)
            {
                foreach (var entry in Answers)
                {
                    var question = await _questionService.GetQuestionByIdAsync(entry.Key);
                    if (question != null)
                    {
                        if (entry.Value?.Trim().Equals(question.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase) == true)
                        {
                            score++;
                        }
                    }
                }
            }

            // 2. حساب النسبة المئوية
            double percentage = totalQuestions > 0 ? Math.Round(((double)score / totalQuestions) * 100, 1) : 0;

            // 3. الحفظ في قاعدة البيانات
            var resultRecord = new UserExamResult
            {
                UserId = User.Identity?.Name ?? "Guest",
                CourseId = model.CourseId,
                ExamType = model.ExamType, // يأتي من الموديل الآن
                TotalQuestions = totalQuestions,
                CorrectAnswers = score,
                ScorePercentage = percentage,
                CompletedAt = DateTime.Now
            };

            _context.UserExamResults.Add(resultRecord);
            await _context.SaveChangesAsync();

            // 4. تجهيز البيانات للعرض
            ViewBag.Score = score;
            ViewBag.Total = totalQuestions;
            ViewBag.Percentage = percentage;

            return View("ExamResult", model);
        }
        // دالة المقارنة الذكية
        private bool IsMatching(string text1, string text2)
        {
            return NormalizeText(text1) == NormalizeText(text2);
        }

        // تنظيف النص من كل العوائق التي تسبب نتيجة 0%
        private string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            return text.Trim()
                .Replace("أ", "ا").Replace("إ", "ا").Replace("آ", "ا") // توحيد الألف
                .Replace("ة", "ه") // توحيد التاء المربوطة
                .Replace("ى", "ي") // توحيد الياء
                .Replace(" ", "")  // إزالة المسافات تماماً لضمان التطابق
                .ToLower();
        }
        [HttpPost]
        public async Task<IActionResult> CohereCommand(string topic, int courseId)
        {
            var dbQuestions = _context.Questions.Where(q => q.CourseId == courseId).ToList();
            ViewBag.Questions = dbQuestions;
            ViewBag.CourseId = courseId;
            ViewBag.SelectedIds = new List<int>();

            if (string.IsNullOrEmpty(topic)) return View("CreateExam");

            var questionsSummary = string.Join("\n", dbQuestions.Select(q => $"ID:{q.QuestionId} Text:{q.QuestionText}"));

            string prompt = $@"Task: Choose question IDs related to '{topic}'.
    Respond ONLY with a JSON array of numbers. 
    Example: [1, 2, 3]
    Questions:
    {questionsSummary}";

            try
            {
                var rawResponse = await _cohereService.GenerateExamQuestions(prompt);

                // هذا السطر هو مفتاح الحل: استخراج المصفوفة فقط باستخدام Regex
                // حتى لو أرسل الـ AI كلاماً مثل "Here are the IDs: [1,2]" سيتم أخذ ما داخل الأقواس فقط
                var match = System.Text.RegularExpressions.Regex.Match(rawResponse, @"\[[\d,\s]+\]");

                if (match.Success)
                {
                    var cleanJson = match.Value;
                    var selectedIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cleanJson);
                    ViewBag.SelectedIds = selectedIds;
                }
                else
                {
                    // إذا لم يجد مصفوفة، قد يكون الرد نصي بحت، نحاول البحث عن أرقام منفردة
                    ViewBag.ErrorMessage = "الذكاء الاصطناعي لم يرسل مصفوفة صحيحة. الرد كان: " + rawResponse;
                }

                return View("CreateExam");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطأ في المعالجة: " + ex.Message;
                return View("CreateExam");
            }
        

    }
        public async Task<IActionResult> MyResults()
        {
            // جلب اسم المستخدم الحالي
            var currentUser = User.Identity.Name;

            // استعلام لجلب النتائج مرتبة من الأحدث إلى الأقدم
            var results = await _context.UserExamResults
                .Where(r => r.UserId == currentUser)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            return View(results);
        }
        public async Task<IActionResult> AdminDashboard()
        {
            var allResults = await _context.UserExamResults
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            // جلب كل الدورات ووضعها في قاموس (Dictionary) ليسهل الوصول لاسم الدورة عبر الـ ID
            var courseNames = await _context.Courses
                .ToDictionaryAsync(c => c.Id, c => c.Name); // تأكد هنا أيضاً من استخدام Name أو Title حسب الموديل

            ViewBag.CourseNames = courseNames;

            // حساب الإحصائيات
            ViewBag.TotalExams = allResults.Count;
            ViewBag.PassRate = allResults.Any()
                ? Math.Round((double)allResults.Count(r => r.ScorePercentage >= 70) / allResults.Count * 100, 1)
                : 0;
            ViewBag.AverageScore = allResults.Any() ? Math.Round(allResults.Average(r => r.ScorePercentage), 1) : 0;
            ViewBag.TopScore = allResults.Any() ? allResults.Max(r => r.ScorePercentage) : 0;

            return View(allResults);
        }
        public async Task<IActionResult> DownloadCertificate(int resultId)
        {
            var result = await _context.UserExamResults.FindAsync(resultId);
            if (result == null) return NotFound();

            if (result.ScorePercentage < 80) return BadRequest("عذراً، لم تجتز النسبة المطلوبة للشهادة");

            var course = await _context.Courses.FindAsync(result.CourseId);
            ViewBag.CourseName = course?.Name ?? "دورة غير معروفة";

            return View("CertificateTemplate", result);
        }
    }
}