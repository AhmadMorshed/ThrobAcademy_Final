using Microsoft.EntityFrameworkCore;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Throb.Repository.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ThrobDbContext _context;

        public QuestionRepository(ThrobDbContext context)
        {
            _context = context;
        }

        public async Task<Question> GetByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionId == id);
        }

        public async Task<List<Question>> GetAllAsync()
        {
            return await _context.Questions
                .Include(q => q.Options)
                .ToListAsync();
        }

        // Throb.Repository/Repositories/QuestionRepository.cs
        public async Task<List<Question>> GetQuestionsByCourseIdAsync(int courseId)
        {
            // تأكد من عمل Include للخيارات لضمان ظهورها في البنك الملون
            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task AddAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync(); // التأكد من وجود هذا السطر
        }

        public async Task UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Question question)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}