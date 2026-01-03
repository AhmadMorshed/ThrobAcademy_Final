using Throb.Data.DbContext;
using Throb.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Throb.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Throb.Repository.Repositories
{
    public class LiveSessionRepository : GenericRepository<LiveSession>, ILiveSessionRepository
    {
        private readonly ThrobDbContext _context;
        private DbSet<LiveSession> DbSet => _context.Set<LiveSession>();

        public LiveSessionRepository(ThrobDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<LiveSession> GetByZoomIdAsync(string zoomMeetingId)
        {
            return await DbSet
                .FirstOrDefaultAsync(ls => ls.ZoomMeetingId.ToString() == zoomMeetingId);
        }

        public async Task AddAttendanceAsync(AttendanceRecord entity)
        {
            if (entity == null) return;
            await _context.Set<AttendanceRecord>().AddAsync(entity);
        }

        // 🟢 الإصلاح هنا: تم تغيير r.UserId إلى r.User
        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceRecordsBySessionIdAsync(int sessionId)
        {
            return await _context.Set<AttendanceRecord>()
                .Include(r => r.User) // ✅ نستخدم اسم الكائن (User) وليس المعرف (UserId) لعمل الربط
                .Where(r => r.LiveSessionId == sessionId)
                .ToListAsync();
        }

        public async Task AddAsync(LiveSession entity)
        {
            await DbSet.AddAsync(entity);
        }

        public async Task DeleteAsync(LiveSession entity)
        {
            DbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<LiveSession>> GetAllAsync()
        {
            return await DbSet.Include(ls => ls.Course).ToListAsync();
        }

        public async Task<LiveSession> GetByIdAsync(int id)
        {
            // يفضل استخدام Include هنا أيضاً إذا كنت تحتاج بيانات الكورس أو الحضور لاحقاً
            return await DbSet.FirstOrDefaultAsync(ls => ls.Id == id);
        }

        public async Task UpdateAsync(LiveSession entity)
        {
            DbSet.Update(entity);
            await Task.CompletedTask;
        }
    }
}