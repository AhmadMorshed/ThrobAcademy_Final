using System.ComponentModel.DataAnnotations;

namespace ThropAcademy.Web.Models
{
    public class LogInViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}