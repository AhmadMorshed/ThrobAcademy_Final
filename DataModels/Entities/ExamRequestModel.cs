using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Throb.Data.Entities
{
    public class ExamRequestModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamRequestId { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "يجب أن يكون عدد الأسئلة بين 1 و50")]
        public int NumberOfQuestions { get; set; }

        [Required]
        public bool IncludeMCQ { get; set; }

        [Required]
        public bool IncludeTrueFalse { get; set; }

        [Required]
        public int EasyCount { get; set; }

        [Required]
        public int MediumCount { get; set; }

        [Required]
        public int HardCount { get; set; }
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        [NotMapped] // هذه أهم خطوة لحل تعارض الـ Migration
        public List<Question> Questions { get; set; } = new List<Question>();

        [NotMapped]
        public List<Question> ManualQuestions { get; set; } = new List<Question>();

        // هذه هي العلاقة الحقيقية الوحيدة التي سيراها EF في قاعدة البيانات
        public virtual ICollection<ExamRequestQuestion> ExamRequestQuestions { get; set; } = new HashSet<ExamRequestQuestion>();
        public string ExamType { get; set; }
        public int DurationMinutes { get; set; }
        // أضف هذا السطر داخل كلاس ExamRequestModel
        
    }
}