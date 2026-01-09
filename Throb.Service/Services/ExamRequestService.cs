using Google;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;

namespace Throb.Service.Services
{
    public class ExamRequestService : IExamRequestService
    {
        private readonly IExamRequestRepository _repository;
        private readonly IQuestionService _questionService;
        private readonly ThrobDbContext _context;

        public ExamRequestService(IExamRequestRepository repository, IQuestionService questionService, ThrobDbContext context)
        {
            _repository = repository;
            _questionService = questionService;
            _context = context;
        }

        public async Task<ExamRequestModel> CreateExamRequestAsync(ExamRequestModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var allQuestions = await _questionService.GetAllQuestionsAsync();
            var examQuestions = new List<Question>();

            if (model.IncludeMCQ)
            {
                var mcqQuestions = allQuestions.Where(q => q.QuestionType == "mcq").ToList();
                examQuestions.AddRange(SelectQuestionsByDifficulty(mcqQuestions, model.EasyCount, model.MediumCount, model.HardCount));
            }
            if (model.IncludeTrueFalse)
            {
                var trueFalseQuestions = allQuestions.Where(q => q.QuestionType == "truefalse").ToList();
                examQuestions.AddRange(SelectQuestionsByDifficulty(trueFalseQuestions, model.EasyCount, model.MediumCount, model.HardCount));
            }

            if (model.Questions == null || !model.Questions.Any())
            {
                model.Questions = examQuestions.OrderBy(q => Guid.NewGuid()).Take(model.NumberOfQuestions).ToList();
            }

            await _repository.AddAsync(model);
            // إعادة جلب السجل للتأكد من أن ExamRequestId تم تعيينه
            return await _repository.GetByIdAsync(model.ExamRequestId);
        }

        public async Task<ExamRequestModel> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<ExamRequestModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task UpdateExamRequestAsync(ExamRequestModel model)
        {
            await _repository.UpdateAsync(model);
        }

        public async Task DeleteExamRequestAsync(int id)
        {
            var examRequest = await _repository.GetByIdAsync(id);
            if (examRequest != null)
            {
                await _repository.DeleteAsync(examRequest);
            }
        }

        public async Task AddManualQuestionAsync(int examId, Question question)
        {
            var examRequest = await _repository.GetByIdAsync(examId);
            if (examRequest != null)
            {
                examRequest.ManualQuestions.Add(question);
                await _repository.UpdateAsync(examRequest);
            }
        }

        private List<Question> SelectQuestionsByDifficulty(List<Question> questions, int easyCount, int mediumCount, int hardCount)
        {
            var easy = questions.Where(q => q.Difficulty == "Easy").OrderBy(q => Guid.NewGuid()).Take(easyCount).ToList();
            var medium = questions.Where(q => q.Difficulty == "Medium").OrderBy(q => Guid.NewGuid()).Take(mediumCount).ToList();
            var hard = questions.Where(q => q.Difficulty == "Hard").OrderBy(q => Guid.NewGuid()).Take(hardCount).ToList();
            return easy.Concat(medium).Concat(hard).ToList();
        }

        public async Task<int> GetExamRequestsCountAsync()
        {
            return await _context.ExamRequestModels.CountAsync();
        }

        public async Task AddAsync(ExamRequestModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. إعداد الكائن الأساسي للتأكد من عدم وجود قيم فارغة
                var newExam = new ExamRequestModel
                {
                    CourseId = model.CourseId,
                    ExamType = model.ExamType,
                    DurationMinutes = model.DurationMinutes,
                    NumberOfQuestions = model.Questions?.Count ?? 0,
                  
                };

                await _context.ExamRequestModels.AddAsync(newExam);
                await _context.SaveChangesAsync(); // حفظ للحصول على RequestId

                // 2. ربط الأسئلة بالامتحان في الجدول الوسيط
                if (model.Questions != null && model.Questions.Any())
                {
                    foreach (var q in model.Questions)
                    {
                        // افترضنا أن اسم الجدول الوسيط هو ExamRequestQuestions
                        // تأكد من مطابقة أسماء الحقول لديك
                        var examQuestion = new
                        {
                            ExamRequestId = newExam.ExamRequestId,
                            QuestionId = q.QuestionId
                        };

                        // إضافة الربط (تعديل حسب اسم الـ DbSet الخاص بالربط عندك)
                        await _context.Database.ExecuteSqlInterpolatedAsync(
                            $"INSERT INTO ExamRequestQuestions (ExamRequestId, QuestionId) VALUES ({newExam.ExamRequestId}, {q.QuestionId})");
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // لإظهار الخطأ الحقيقي في الـ Debugger
            }
        }
    }
}