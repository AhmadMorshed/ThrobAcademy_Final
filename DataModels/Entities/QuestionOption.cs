using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Data.Entities
{
    public class QuestionOption
    {
        [Key]
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; }
        public Question Question { get; set; }
    }
}
