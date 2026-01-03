using Throb.Data.Entities;
using Throb.Service.Models;

public interface ILiveSession
{
    Task<LiveSession> GetByIdAsync(int id);
    Task<IEnumerable<LiveSession>> GetAllAsync();
    Task AddAsync(LiveSession liveSession);
    Task UpdateAsync(LiveSession liveSession);
    Task DeleteAsync(LiveSession liveSession);

    // Zoom Core
    Task<LiveSession> CreateZoomSessionAsync(LiveSession session, string masterEmail);
    Task<LiveSession> GetSessionByZoomIdAsync(string zoomMeetingId);

    // Attendance Logic
    Task<int> RecordAttendanceAsync(int liveSessionId);
    Task<IEnumerable<AttendanceRecord>> GetAttendanceRecordsBySessionIdAsync(int sessionId);
    Task<ZoomMeetingParticipantsResponse> FetchZoomReportData(string zoomMeetingId);

    // Logging (Webhooks)
    Task AddAttendanceLogAsync(AttendanceLog logEntry);
}