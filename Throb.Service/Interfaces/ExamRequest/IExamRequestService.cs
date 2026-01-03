using Throb.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Throb.Service.Interfaces
{
    public interface IExamRequestService
    {
        Task<ExamRequestModel> CreateExamRequestAsync(ExamRequestModel model);
        Task<ExamRequestModel> GetByIdAsync(int id);
        Task<List<ExamRequestModel>> GetAllAsync();
        Task UpdateExamRequestAsync(ExamRequestModel model);
        Task DeleteExamRequestAsync(int id);
        Task AddManualQuestionAsync(int examId, Question question);
    }
}