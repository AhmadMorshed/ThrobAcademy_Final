namespace ThropAcademy.Web.Models
{
    public class LiveSessionViewModel
    {
        public IEnumerable<Throb.Data.Entities.LiveSession> LiveSessions { get; set; }
        public IEnumerable<Throb.Data.Entities.Course> Courses { get; set; }

        public string Link { get; set; }
    }
}
