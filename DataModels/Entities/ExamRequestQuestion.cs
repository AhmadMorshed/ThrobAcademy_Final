using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Data.Entities
{
    public class ExamRequestQuestion
    {
        public int ExamRequestId { get; set; }
        public ExamRequestModel ExamRequest { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
