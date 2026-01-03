using System;
using Throb.Data.Entities; // إذا كانت الدورة في هذا المجلد
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ⚠️ تأكد من إضافة هذا النموذج إلى DbContext (DbSet<LectureResource>)
public class LectureResource
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; }

    [Required]
    public string FilePath { get; set; }

    [Required]
    [MaxLength(50)]
    public string MimeType { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.Now;

    // الخصائص الأجنبية
    public int CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course Course { get; set; } // ⚠️ تأكد من أن Course مُعرّف في Throb.Data.Entities
}