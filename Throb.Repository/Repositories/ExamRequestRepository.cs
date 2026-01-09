using Microsoft.EntityFrameworkCore;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Throb.Repository.Repositories
{
    public class ExamRequestRepository : IExamRequestRepository
    {
        private readonly ThrobDbContext _context;

        public ExamRequestRepository(ThrobDbContext context)
        {
            _context = context;
        }

        public async Task<ExamRequestModel> GetByIdAsync(int id)
        {
            // نستخدم Include للجدول الوسيط ثم ThenInclude للوصول للسؤال وخياراته
            return await _context.ExamRequestModels
                .Include(er => er.ExamRequestQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(er => er.ExamRequestId == id);
        }

        public async Task<List<ExamRequestModel>> GetAllAsync()
        {
            return await _context.ExamRequestModels
                .Include(er => er.ExamRequestQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q.Options)
                .ToListAsync();
        }

        public async Task AddAsync(ExamRequestModel examRequest)
        {
            await _context.ExamRequestModels.AddAsync(examRequest);
            await SaveChangesAsync();

            if (examRequest.ExamRequestId == 0)
            {
                throw new InvalidOperationException("Failed to generate ExamRequestId.");
            }
        }

        public async Task UpdateAsync(ExamRequestModel examRequest)
        {
            _context.ExamRequestModels.Update(examRequest);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(ExamRequestModel examRequest)
        {
            _context.ExamRequestModels.Remove(examRequest);
            await SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}