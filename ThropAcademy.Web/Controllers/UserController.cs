using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Throb.Data.Entities;
using ThropAcademy.Web.Models;

namespace ThropAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]

    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger; 

        public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchInp)
        {
            List<ApplicationUser> users;

            if (string.IsNullOrEmpty(searchInp))
            {
                users = await _userManager.Users.ToListAsync();
            }
            else
            {
               
                string normalizedSearch = searchInp.Trim().ToUpper();
                users = await _userManager.Users
                    .Where(user => user.NormalizedEmail.Contains(normalizedSearch) ||
                                   user.NormalizedUserName.Contains(normalizedSearch))
                    .ToListAsync();
            }

            return View(users);
        }

        public async Task<IActionResult> Details(string id, string viewName = "Details")
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            if (viewName == "Update")
            {
                var userViewModel = new UserUpdateViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,

                };
                
                return View(viewName, userViewModel);
            }

            return View(viewName, user);
        }

        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            return await Details(id, "Update");
        }
        [HttpPost]
        public async Task<IActionResult> Update(string? id, UserUpdateViewModel applicationUser)
        {
            if (id != applicationUser.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user is null)
                        return NotFound();

                    user.UserName = applicationUser.UserName;
                   

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User Update Successfully");
                        return RedirectToAction("Index");

                    }
                    foreach (var item in result.Errors)
                    {
                        _logger.LogError(item.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user: {Message}", ex.Message);
                }
            }
            return View(applicationUser);

        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                    return NotFound();
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    
                    _logger.LogInformation("User Deleted Successfully");
                    return RedirectToAction("Index");
                }

                foreach (var item in result.Errors)
                {
                    _logger.LogError(item.Description);
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting user: {Message}", e.Message);
            }
            return RedirectToAction("Index");

        }
    }
}