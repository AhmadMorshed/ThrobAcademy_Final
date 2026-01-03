using Microsoft.EntityFrameworkCore;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Throb.Repository.Repositories
{
    public class DriveSessionRepository : GenericRepository<DriveSession>, IDriveSessionRepository
    {
        private readonly ThrobDbContext _context;

        public DriveSessionRepository(ThrobDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DriveSession> GetByIdAsync(int id)
        {
            return await _context.DriveSessions
                .Include(ds => ds.Courses)
                .FirstOrDefaultAsync(ds => ds.Id == id);
        }

        public async Task<IEnumerable<DriveSession>> GetByCourseIdAsync(int courseId)
        {
            return await _context.DriveSessions
                .Include(ds => ds.Courses)
                .Where(ds => ds.Courses.Any(c => c.Id == courseId))
                .ToListAsync();
        }
    }
}