using Throb.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Throb.Repository.Interfaces
{
    public interface ILiveSessionRepository : IGenericRepository<LiveSession>
    {
        Task<LiveSession> GetByIdAsync(int id);
        Task<IEnumerable<LiveSession>> GetAllAsync();
        Task AddAsync(LiveSession entity);
        Task UpdateAsync(LiveSession entity);
        Task DeleteAsync(LiveSession entity);

        // 🟢 دوال Zoom/Attendance
        Task<LiveSession> GetByZoomIdAsync(string zoomMeetingId); // للبحث عن الجلسة عبر الـ Webhook
        Task AddAttendanceAsync(AttendanceRecord entity); // لإضافة سجل الحضور
        Task<IEnumerable<AttendanceRecord>> GetAttendanceRecordsBySessionIdAsync(int sessionId);
    }
}