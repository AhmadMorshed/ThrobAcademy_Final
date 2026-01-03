using Throb.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Throb.Repository.Interfaces
{
    public interface IExamRequestRepository
    {
        Task<ExamRequestModel> GetByIdAsync(int id);
        Task<List<ExamRequestModel>> GetAllAsync();
        Task AddAsync(ExamRequestModel examRequest);
        Task UpdateAsync(ExamRequestModel examRequest);
        Task DeleteAsync(ExamRequestModel examRequest);
        Task<int> SaveChangesAsync();
    }
}