using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Throb.Data.Entities
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string QuestionType { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        [Required]
        public string Difficulty { get; set; }

        [Required]
        public string Transcript { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CourseId { get; set; }

        public List<QuestionOption> Options { get; set; }
        //public List<ExamRequestModel> ExamRequests { get; set; } = new List<ExamRequestModel>();
        public virtual ICollection<ExamRequestQuestion> ExamRequestQuestions { get; set; } = new HashSet<ExamRequestQuestion>();
        public bool IsManual { get; set; } = false;
    }
}