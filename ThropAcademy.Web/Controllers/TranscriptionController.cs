using Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Neuroglia.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Service.Interfaces;
using Throb.Service.Interfaces.GeminiAI;
using Throb.Service.Services;
using Throb.Services;
using ThropAcademy.Web.Models;


namespace Throb.Controllers
{

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
        [Authorize(Roles = "Admin,Instructor")]

        private void SafeDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch {  }
        }
        #endregion

        #region Operations: Upload & Transcription
        [Authorize(Roles = "Admin,Instructor")]

        [HttpGet]
        public async Task<IActionResult> UploadMedia(int? courseId)
        {
            ViewBag.Courses = await _questionService.GetAllCoursesForSelectionAsync();
            ViewBag.SelectedCourseId = courseId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]

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
        [Authorize(Roles = "Admin,Instructor")]

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
        [Authorize(Roles = "Admin,Instructor")]

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

               
                var simplifiedQuestions = allQuestions.Select(q => new
                {
                    id = q.QuestionId,
                    diff = q.Difficulty,
                    txt = q.QuestionText
                }).ToList();

                var questionsJson = JsonSerializer.Serialize(simplifiedQuestions);

                var selectedIds = await _geminiService.GetSmartSelectionAsync(userPrompt, questionsJson);

                
                if (selectedIds == null || !selectedIds.Any())
                {
                    TempData["ErrorMessage"] = "عذراً، لم أستطع فهم الطلب أو لم أجد أسئلة مناسبة. حاول تبسيط العبارة.";
                    return RedirectToAction("CreateExam", new { courseId = courseId });
                }

               
                var finalValidIds = selectedIds.Intersect(allQuestions.Select(q => q.QuestionId)).ToList();

               
                TempData["SelectedIds"] = JsonSerializer.Serialize(finalValidIds);

                return RedirectToAction("CreateExam", new { courseId = courseId });
            }
            catch (Exception ex)
            {
                
                TempData["ErrorMessage"] = "حدث خطأ تقني أثناء الاتصال بالذكاء الاصطناعي. يرجى المحاولة لاحقاً.";
                return RedirectToAction("CreateExam", new { courseId = courseId });
            }
        }
        #endregion

        #region Management: Question Bank & Exams
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]

        public async Task<IActionResult> QuestionBank(int? courseId)
        {
            int targetId = courseId ?? 0;
           
            var questions = await _questionService.GetQuestionsByCourseAsync(targetId);
            var course = _courseService.GetById(targetId);

            ViewBag.CourseId = targetId;
            ViewBag.CourseName = course?.Name ?? "كورس غير محدد";

            return View(questions);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateExam(int courseId)
        {
            // جلب الأسئلة مع خياراتها لعرضها في قائمة الاختيار إذا لزم الأمر
            var questions = await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            ViewBag.Questions = questions;
            ViewBag.CourseId = courseId;

            if (TempData["SelectedIds"] != null)
            {
                ViewBag.SelectedIds = JsonSerializer.Deserialize<List<int>>(TempData["SelectedIds"].ToString());
            }

            return View(new ExamRequestModel { CourseId = courseId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateExam(ExamRequestModel model, List<int> SelectedQuestionIds)
        {
            if (SelectedQuestionIds == null || !SelectedQuestionIds.Any())
            {
                TempData["ErrorMessage"] = "يرجى اختيار سؤال واحد على الأقل.";
                return RedirectToAction("CreateExam", new { courseId = model.CourseId });
            }

            try
            {
                // 1. حفظ رأس الاختبار (ExamRequestModel)
                _context.ExamRequestModels.Add(model);
                await _context.SaveChangesAsync();

                // 2. حفظ تفاصيل الاختبار (الجدول الوسيط)
                foreach (var qId in SelectedQuestionIds)
                {
                    var examQuestion = new ExamRequestQuestion
                    {
                        ExamRequestId = model.ExamRequestId,
                        QuestionId = qId
                    };
                    _context.ExamRequestQuestions.Add(examQuestion);
                }
                await _context.SaveChangesAsync();

                // 3. جلب الأسئلة مع خياراتها (Include) لغرض العرض في صفحة المعاينة
                // هذا السطر هو المسؤول عن حل مشكلة اختفاء خيارات الـ MCQ
                var questionsWithOptions = await _context.Questions
                    .Include(q => q.Options)
                    .Where(q => SelectedQuestionIds.Contains(q.QuestionId))
                    .ToListAsync();

                // تمرير الأسئلة المحملة بالكامل للموديل قبل إرساله للـ View
                model.Questions = questionsWithOptions;

                return View("ExamPreview", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "حدث خطأ أثناء حفظ الاختبار: " + ex.Message;

                // إعادة تحميل البيانات للـ View في حالة الخطأ
                ViewBag.Questions = await _context.Questions.Where(q => q.CourseId == model.CourseId).ToListAsync();
                ViewBag.CourseId = model.CourseId;

                return View(model);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> ConfirmAndSaveExam(ExamRequestModel model, List<int> SelectedQuestionIds)
        {
            try
            {
                // 1. إعادة ربط الأسئلة المختارة بالموديل قبل الحفظ
                if (SelectedQuestionIds != null && SelectedQuestionIds.Any())
                {
                    model.Questions = await _context.Questions
                        .Where(q => SelectedQuestionIds.Contains(q.QuestionId))
                        .ToListAsync();
                }

                // 2. استدعاء خدمة الحفظ الفعلي في قاعدة البيانات
                await _examRequestService.AddAsync(model);

                TempData["SuccessMessage"] = "تم حفظ الاختبار ونشره بنجاح!";
                return RedirectToAction("AvailableExams", "Exam");
            }
            catch (Exception ex)
            {
                return BadRequest("خطأ أثناء الحفظ النهائي: " + ex.Message);
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
        public async Task<IActionResult> SubmitExam(ExamSubmissionViewModel submission)
        {
            if (submission == null || submission.Answers == null)
            {
                return BadRequest("بيانات الإرسال غير مكتملة.");
            }

            try
            {
                // 1. جلب بيانات طلب الاختبار للحصول على نوع الاختبار (ExamType)
                // هذا يحل مشكلة الخطأ "Cannot insert NULL into ExamType"
                var examRequest = await _context.ExamRequestModels
                    .FirstOrDefaultAsync(e => e.ExamRequestId == submission.ExamRequestId);

                if (examRequest == null) return NotFound("طلب الاختبار غير موجود.");

                // 2. جلب الأسئلة الأصلية من قاعدة البيانات لمقارنة الإجابات
                var questionIds = submission.Answers.Select(a => a.QuestionId).ToList();
                var questions = await _context.Questions
                    .Where(q => questionIds.Contains(q.QuestionId))
                    .ToListAsync();

                // 3. حساب عدد الإجابات الصحيحة
                int correctCount = 0;
                foreach (var answer in submission.Answers)
                {
                    var originalQuestion = questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                    if (originalQuestion != null)
                    {
                        // مقارنة نص الخيار المختار مع الإجابة الصحيحة المخزنة
                        if (originalQuestion.CorrectAnswer.Trim().ToLower() == answer.SelectedOption?.Trim().ToLower())
                        {
                            correctCount++;
                        }
                    }
                }

                // 4. حساب النسبة المئوية
                double scorePercentage = questions.Any() ? (double)correctCount / questions.Count * 100 : 0;

                // 5. إنشاء سجل النتيجة وحفظه في جدول UserExamResults
                var result = new UserExamResult
                {
                    ExamRequestId = submission.ExamRequestId,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier), // معرف المستخدم المسجل حالياً
                    CourseId = submission.CourseId,
                    ExamType = examRequest.ExamType ?? "Quiz", // ضمان عدم إرسال NULL لقاعدة البيانات
                    TotalQuestions = questions.Count,
                    CorrectAnswers = correctCount,
                    ScorePercentage = scorePercentage,
                    CompletedAt = DateTime.Now
                };

                _context.UserExamResults.Add(result);
                await _context.SaveChangesAsync();

                // التوجه لصفحة النجاح وعرض النتيجة
                return RedirectToAction("ExamResult", new { id = result.Id });
            }
            catch (Exception ex)
            {
                // إظهار الخطأ الحقيقي (InnerException) إذا وجد لسهولة التصحيح
                var message = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"حدث خطأ أثناء حفظ النتائج: {message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ExamResult(int id)
        {
            var result = await _context.UserExamResults
                .Include(r => r.Course) // سيعمل الآن إذا أضفت الخاصية للموديل
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null) return NotFound();

            // هنا نرسل 'result' وهو من نوع UserExamResult
            return View(result);
        }

        private bool IsMatching(string text1, string text2)
        {
            return NormalizeText(text1) == NormalizeText(text2);
        }

       
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

               
                var match = System.Text.RegularExpressions.Regex.Match(rawResponse, @"\[[\d,\s]+\]");

                if (match.Success)
                {
                    var cleanJson = match.Value;
                    var selectedIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cleanJson);
                    ViewBag.SelectedIds = selectedIds;
                }
                else
                {
                    
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
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> MyResults()
        {
       
            var currentUser = User.Identity.Name;

            
            var results = await _context.UserExamResults
                .Where(r => r.UserId == currentUser)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            return View(results);
        }
        [Authorize(Roles = "Admin,Instructor")]

        public async Task<IActionResult> AdminDashboard()
        {
            var allResults = await _context.UserExamResults
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

     
            var courseNames = await _context.Courses
                .ToDictionaryAsync(c => c.Id, c => c.Name); 
            ViewBag.CourseNames = courseNames;

            
            ViewBag.TotalExams = allResults.Count;
            ViewBag.PassRate = allResults.Any()
                ? Math.Round((double)allResults.Count(r => r.ScorePercentage >= 70) / allResults.Count * 100, 1)
                : 0;
            ViewBag.AverageScore = allResults.Any() ? Math.Round(allResults.Average(r => r.ScorePercentage), 1) : 0;
            ViewBag.TopScore = allResults.Any() ? allResults.Max(r => r.ScorePercentage) : 0;

            return View(allResults);
        }
        [Authorize(Roles = "Admin,Instructor,Student")]

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