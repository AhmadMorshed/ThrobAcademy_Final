using System;
using Throb.Data.Entities; // إذا كانت الدورة في هذا المجلد
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public int CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course Course { get; set; } 
}