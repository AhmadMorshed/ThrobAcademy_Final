using Throb.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Throb.Repository.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question> GetByIdAsync(int id);
        Task<List<Question>> GetAllAsync();
        Task<List<Question>> GetQuestionsByCourseIdAsync(int courseId);
        Task<List<Course>> GetAllCoursesAsync(); // جلب الكورسات لغرض القائمة المنسدلة
        Task AddAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(Question question);
        Task<int> SaveChangesAsync();
    }
}