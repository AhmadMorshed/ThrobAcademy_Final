    using Microsoft.EntityFrameworkCore;
    using Throb.Data.DbContext;
    using Throb.Data.Entities;
    using Throb.Repository.Interfaces;
    using System.Collections.Generic;
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
                return await _context.ExamRequestModels
                    .Include(er => er.Questions)
                    .ThenInclude(q => q.Options) // إذا كنت تريد تضمين الخيارات
                    .FirstOrDefaultAsync(er => er.ExamRequestId == id);
            }

            public async Task<List<ExamRequestModel>> GetAllAsync()
            {
                return await _context.ExamRequestModels
                    .Include(er => er.Questions)
                    .ThenInclude(q => q.Options)
                    .ToListAsync();
            }

        public async Task AddAsync(ExamRequestModel examRequest)
        {
            await _context.ExamRequestModels.AddAsync(examRequest);
            await SaveChangesAsync();
            // تحقق من أن ExamRequestId تم تعيينه
            if (examRequest.ExamRequestId == 0)
            {
                throw new InvalidOperationException("Failed to generate ExamRequestId. Check database configuration.");
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