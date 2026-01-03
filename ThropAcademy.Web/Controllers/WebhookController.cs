using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json; // لاستخدام JsonSerializer
using Throb.Data.Entities; // 💡 تأكد من أن هذا هو مسار نموذج AttendanceLog
using Throb.Service.Interfaces;
using Throb.Service.Models;
using ThropAcademy.Web.Models;

[ApiController]
[Route("zoom/webhooks")] // يجب أن يطابق هذا المسار ما هو مسجل في Zoom
public class WebhookController : ControllerBase
{
    private readonly ILiveSession _liveSessionService;

    public WebhookController(ILiveSession liveSessionService)
    {
        _liveSessionService = liveSessionService;
    }

    [HttpPost]
    // 💡 إزالة [FromBody] لقراءة الحمولة الخام يدوياً
    public async Task<IActionResult> HandleEvent()
    {
        // 🚨 الخطوة 0: قراءة الحمولة الخام (Raw Body)
        var rawPayload = await ReadRawBody(HttpContext.Request);
        if (string.IsNullOrEmpty(rawPayload)) return BadRequest();

        // -------------------------------------------------------------
        // 🛠️ الخطوة 1: معالجة طلب التحقق (URL Validation)
        // -------------------------------------------------------------

        try
        {
            // محاولة فك تشفير الحمولة إلى نموذج التحقق
            var validationRequest = JsonSerializer.Deserialize<ZoomValidationRequest>(rawPayload);

            if (validationRequest?.Event == "endpoint.url_validation")
            {
                // إذا كان طلب تحقق، نُنشئ الرد المطلوب من Zoom
                var response = new { plainToken = validationRequest.PlainToken, encryptedToken = validationRequest.EncryptedToken };
                System.Diagnostics.Debug.WriteLine($"[WEBHOOK VALIDATION] Responding to Zoom Challenge.");

                return Ok(response); // 🟢 نُعيد الرد المطلوب بـ 200 OK
            }
        }
        catch (Exception)
        {
            // نتجاهل أي خطأ في فك التشفير هنا ونفترض أنه قد يكون حدثاً عادياً
        }

        // -------------------------------------------------------------
        // ⚙️ الخطوة 2: معالجة الأحداث العادية (Attendance Events)
        // -------------------------------------------------------------

        try
        {
            // فك تشفير الحمولة الخام إلى نموذج ZoomWebhookPayload
            var payload = JsonSerializer.Deserialize<ZoomWebhookPayload>(rawPayload);

            if (payload?.Event == null || payload.Payload == null) return BadRequest();

            // 1. نحن مهتمون فقط بأحداث الدخول والخروج 
            if (payload.Event == "meeting.participant_joined" || payload.Event == "meeting.participant_left")
            {
                var zoomMeetingId = payload.Payload.Id;
                var participant = payload.Payload.Participant;
                var eventType = payload.Event == "meeting.participant_joined" ? "joined" : "left";

                // 2. البحث عن الجلسة
                var liveSession = await _liveSessionService.GetSessionByZoomIdAsync(zoomMeetingId);

                if (liveSession == null)
                {
                    // 🛑 إذا لم يتم العثور على الجلسة، قم بتسجيل المحاولة
                    System.Diagnostics.Debug.WriteLine($"[WEBHOOK ERROR] Session not found for Zoom ID: {zoomMeetingId}");
                    return Ok();
                }

                // 3. إنشاء وحفظ السجل (تم إصلاح خطأ 'logEntry' هنا)
                var logEntry = new AttendanceLog // 👈 تم تعريف المتغير logEntry هنا
                {
                    LiveSessionId = liveSession.Id,
                    ZoomMeetingId = zoomMeetingId,
                    ParticipantEmail = participant.UserEmail,
                    EventType = eventType,
                    EventTimestamp = DateTime.UtcNow
                };

                await _liveSessionService.AddAttendanceLogAsync(logEntry);

                System.Diagnostics.Debug.WriteLine($"[WEBHOOK SUCCESS] Saved log for {participant.UserEmail} event: {eventType}");
            }
        }
        catch (Exception ex)
        {
            // 🚨 تسجيل أي خطأ يحدث أثناء معالجة الحدث العادي (Event)
            System.Diagnostics.Debug.WriteLine($"[WEBHOOK CRASH] Fatal Exception during processing: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[WEBHOOK CRASH] Inner Exception: {ex.InnerException?.Message}");
        }

        // 🟢 العودة بـ 200 OK فوراً (إلزامي لـ Zoom)
        return Ok();
    }

    // -------------------------------------------------------------
    // 🧩 الدوال والنماذج المساعدة
    // -------------------------------------------------------------

    private async Task<string> ReadRawBody(HttpRequest request)
    {
        // ⚠️ مهم جداً: لتجنب الخطأ "Reading stream failed"
        request.EnableBuffering();

        using (var reader = new StreamReader(request.Body, Encoding.UTF8))
        {
            var rawPayload = await reader.ReadToEndAsync();
            request.Body.Position = 0; // إعادة تعيين المؤشر ليتمكن الكود التالي من قراءة الحمولة مرة أخرى
            return rawPayload;
        }
    }

    // نموذج بيانات التحقق (URL Validation) من Zoom

    // 💡 يجب أن يكون نموذج ZoomWebhookPayload موجوداً لديك بالفعل في مكان ما 
    // public class ZoomWebhookPayload { ... }
}