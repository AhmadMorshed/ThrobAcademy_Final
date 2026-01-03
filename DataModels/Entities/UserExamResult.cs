using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Data.Entities
{
    public class UserExamResult
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CourseId { get; set; }

        // الخاصية الجديدة لتخزين نوع الامتحان (Quiz or Final)
        public string ExamType { get; set; }

        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double ScorePercentage { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}