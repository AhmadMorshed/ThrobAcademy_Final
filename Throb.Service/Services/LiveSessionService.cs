using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces;
using Throb.Service.Models;

namespace Throb.Service.Services
{
    public class LiveSessionService : ILiveSession
    {
        private readonly ILiveSessionRepository _liveSessionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentService _studentService;
        private readonly IZoomAuthService _zoomAuthService;
        private readonly HttpClient _httpClient;
        private readonly ThrobDbContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;

        public LiveSessionService(
            ILiveSessionRepository liveSessionRepository,
            ICourseRepository courseRepository,
            IStudentService studentService,
            IZoomAuthService zoomAuthService,
            HttpClient httpClient,
            ThrobDbContext context,
            IConfiguration config,
            UserManager<ApplicationUser> userManager)
        {
            _liveSessionRepository = liveSessionRepository;
            _courseRepository = courseRepository;
            _studentService = studentService;
            _zoomAuthService = zoomAuthService;
            _httpClient = httpClient;
            _context = context;
            _config = config;
            _userManager = userManager;
        }

        #region Attendance Logic (تسجيل الحضور)

        // الدالة التي يتم استدعاؤها من الزر الأصفر في الواجهة
        public async Task<int> RecordAttendanceAsync(int liveSessionId)
        {
            var session = await _context.LiveSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == liveSessionId);

            if (session == null) return 0;

            // محاكاة بيانات قادمة من زووم لتجاوز قيد الحساب المجاني (Mock Data)
            var fakeZoomParticipants = new List<ParticipantResponse>
            {
                new ParticipantResponse { name = "أحمد محمد", user_email = "ahmed@example.com", duration = 2700 }, // 45 min
                new ParticipantResponse { name = "سارة أحمد", user_email = "sara@example.com", duration = 3600 },  // 60 min
                new ParticipantResponse { name = "محمود علي", user_email = "mahmoud@example.com", duration = 1200 } // 20 min
            };

            return await ProcessAttendanceData(session, fakeZoomParticipants);
        }

        // معالجة البيانات وحفظها في قاعدة البيانات مع ربط الـ UserId
        private async Task<int> ProcessAttendanceData(LiveSession session, List<ParticipantResponse> participants)
        {
            int addedCount = 0;

            foreach (var participant in participants)
            {
                // 1. فحص هل السجل موجود مسبقاً لهذه الجلسة وهذا الإيميل؟
                bool exists = await _context.AttendanceRecords
                    .AnyAsync(r => r.LiveSessionId == session.Id && r.ParticipantEmail == participant.user_email);

                if (exists) continue; // إذا كان موجوداً، تخطاه ولا تضفه مرة أخرى

                int durationInMinutes = participant.duration / 60;
                var user = await _userManager.FindByEmailAsync(participant.user_email);

                var attendanceRecord = new AttendanceRecord
                {
                    LiveSessionId = session.Id,
                    ParticipantName = participant.name.Length > 100 ? participant.name.Substring(0, 99) : participant.name, // حماية من طول النص
                    ParticipantEmail = participant.user_email,
                    DurationMinutes = durationInMinutes,
                    UserId = user?.Id,
                    RecordedAt = DateTime.Now
                };

                await _context.AttendanceRecords.AddAsync(attendanceRecord);
                addedCount++;
            }

            if (addedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return addedCount;
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAttendanceRecordsBySessionIdAsync(int sessionId)
        {
            return await _liveSessionRepository.GetAttendanceRecordsBySessionIdAsync(sessionId);
        }

        #endregion

        #region Zoom API Integration (الربط مع زووم)

        public async Task<LiveSession> CreateZoomSessionAsync(LiveSession session, string masterEmail)
        {
            var token = await _zoomAuthService.GetAccessTokenAsync();
            // داخل دالة CreateZoomSessionAsync في ملف LiveSessionService.cs
            var requestModel = new ZoomCreateMeetingRequest
            {
                Topic = session.Title,
                Type = 2,
                StartTime = session.Date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Duration = session.DurationMinutes,
                UserId = masterEmail,
                Settings = new MeetingSettings
                {
                    Password = session.Password,
                    HostVideo = true,
                    JoinBeforeHost = true,
                    LocalRecording = false, // موجودة في الكلاس الخاص بك
                    AutoRecording = "none"  // موجودة في الكلاس الخاص بك
                                            // ❌ لا تضع ParticipantVideo هنا إلا إذا أضفتها للكلاس أولاً
                }
            };
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.zoom.us/v2/users/{masterEmail}/meetings");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var zoomResponse = JsonSerializer.Deserialize<ZoomMeetingResponse>(content);

            session.ZoomMeetingId = zoomResponse.Id;
            session.StartUrl = zoomResponse.StartUrl;
            session.JoinUrl = zoomResponse.JoinUrl;
            session.Password = zoomResponse.Password;

            await _liveSessionRepository.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ZoomMeetingParticipantsResponse> FetchZoomReportData(string zoomMeetingId)
        {
            var token = await _zoomAuthService.GetAccessTokenAsync();
            var url = $"https://api.zoom.us/v2/past_meetings/{zoomMeetingId}/participants?page_size=300";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ZoomMeetingParticipantsResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        #endregion

        #region CRUD Operations (عمليات الإدارة)

        public async Task<LiveSession> GetByIdAsync(int id) => await _liveSessionRepository.GetByIdAsync(id);

        public async Task<IEnumerable<LiveSession>> GetAllAsync() => await _liveSessionRepository.GetAllAsync();

        public async Task AddAsync(LiveSession liveSession)
        {
            await _liveSessionRepository.AddAsync(liveSession);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LiveSession liveSession)
        {
            await _liveSessionRepository.UpdateAsync(liveSession);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LiveSession liveSession)
        {
            await _liveSessionRepository.DeleteAsync(liveSession);
            await _context.SaveChangesAsync();
        }

        public async Task<LiveSession> GetSessionByZoomIdAsync(string zoomMeetingId)
        {
            return await _liveSessionRepository.GetByZoomIdAsync(zoomMeetingId);
        }

        public async Task AddAttendanceLogAsync(AttendanceLog logEntry)
        {
            await _context.Set<AttendanceLog>().AddAsync(logEntry);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}