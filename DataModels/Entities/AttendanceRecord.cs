using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Throb.Data.Entities;

public class AttendanceRecord
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("LiveSession")]
    public int LiveSessionId { get; set; }
    public LiveSession LiveSession { get; set; }

    // 🟢 التعديل: الربط مع المستخدم العام بدل الطالب
    [ForeignKey("User")]
    public string UserId { get; set; } // غالباً يكون string إذا كنت تستخدم Identity
    public ApplicationUser User { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    [MaxLength(255)]
    public string ParticipantName { get; set; }

    [MaxLength(255)]
    public string ParticipantEmail { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.Now;
}