using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Throb.Data.Entities;

namespace Throb.Service.Interfaces
{
    public interface IDriveSessionService
    {
        Task AddAsync(IFormFile file, int[] courseIds, string title);
        void Delete(DriveSession driveSession);
        IEnumerable<DriveSession> GetAll();
        DriveSession? GetById(int? id);
        Task<IEnumerable<DriveSession>> GetByCourseId(int courseId);
        void Update(DriveSession driveSession);
        Task<DriveSession?> GetByIdAsync(int id);
        Task<string> SaveDocumentAsync(IFormFile file);
    }
}