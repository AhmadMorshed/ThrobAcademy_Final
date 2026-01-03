using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Throb.Data.Entities;
using ThropAcademy.Web.Models;

namespace ThropAcademy.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // صفحة التسجيل
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel input)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = input.Email.Split("@")[0],
                    Email = input.Email,
                    Firstname = input.FirstName,
                    Lastname = input.LastName,
                    IsActive = true  // تأكد من أن هذا متوافق مع سياسة الحساب لديك
                };

                var result = await _userManager.CreateAsync(user, input.Password);

                if (result.Succeeded)
                {
                    // بعد النجاح، يمكنك تسجيل الدخول مباشرة أو إرسال بريد تأكيد، إلخ.
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }

        // صفحة تسجيل الدخول
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LogInViewModel input)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(input.Email);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, input.Password, input.RememberMe, false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in: {Email}", input.Email);  // سجل الدخول الناجح
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogWarning("Login attempt failed for user: {Email}", input.Email);  // سجل فشل الدخول
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(input);
        }

        // الخروج من الحساب
        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");  // إعادة توجيه إلى الصفحة الرئيسية
        }

        // صفحة الوصول المرفوض
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
