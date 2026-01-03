using System.Collections.Generic;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Service.Interfaces
{
    public interface IQuestionService
    {
        Task<string> GenerateAndStoreQuestionsAsync(string text, string type, int courseId);
        Task<List<Question>> GetQuestionsByCourseAsync(int courseId);
        Task<List<Question>> GetAllQuestionsAsync();
        Task<List<Course>> GetAllCoursesForSelectionAsync();
        Task<Question> GetQuestionByIdAsync(int id);
    }
}