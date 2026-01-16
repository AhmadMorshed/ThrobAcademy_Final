namespace ThropAcademy.Web.Models
{
    public class UserUpdateViewModel
    {
        public string Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? NewPassword { get; set; } // حقل إضافي لتغيير كلمة المرور
        public bool LockoutEnabled { get; set; } // حقل إضافي لحالة القفل
    }
}