using System;

namespace Throb.Data.Entities
{
    public class AttendanceLog
    {
        public int Id { get; set; }

        public int LiveSessionId { get; set; } // يجب ربطها يدوياً

        public string? ZoomMeetingId { get; set; } // معرّف الاجتماع من Zoom

        public string? ParticipantEmail { get; set; } // بريد الطالب

        public string? EventType { get; set; } // "joined" أو "left"

        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
    }
}