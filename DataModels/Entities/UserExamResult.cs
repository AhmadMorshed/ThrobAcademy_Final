using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Data.Entities
{
    public class UserExamResult
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } // أو IdentityUser
        public int CourseId { get; set; }
        public int ExamRequestId { get; set; }
        // الخاصية الجديدة لتخزين نوع الامتحان (Quiz or Final)
        public string ExamType { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double ScorePercentage { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}