using System.Collections.Generic;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Repository.Interfaces
{
    public interface IDriveSessionRepository : IGenericRepository<DriveSession>
    {
        Task<IEnumerable<DriveSession>> GetByCourseIdAsync(int courseId);
        Task<DriveSession?> GetByIdAsync(int id);
    }
}