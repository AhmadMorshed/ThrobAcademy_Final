using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Throb.Service.Models
{
    public class ParticipantResponse
    {
        public string name { get; set; }
        public string user_email { get; set; }
        public int duration { get; set; } // المدة بالثواني أو الدقائق حسب ما ترسل
    }
}
