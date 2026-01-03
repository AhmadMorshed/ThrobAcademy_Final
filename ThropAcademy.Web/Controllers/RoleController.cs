using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Throb.Data.Entities;
using ThropAcademy.Web.Models;

namespace ThropAcademy.Web.Controllers
{
    [Authorize(Roles = "Admin")]

    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to initialize RoleManager and Logger
        public RoleController(RoleManager<IdentityRole> roleManager, ILogger<RoleController> logger, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _logger = logger;
            _userManager = userManager;
        }

        // Get all roles and pass them to the View
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles); // Pass the roles to the View
        }

        // Display create role page
        public IActionResult Create()
        {
            return View(new IdentityRole()); // Passing an empty IdentityRole to the view
        }

        // Handle create role post request
        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                foreach (var item in result.Errors)
                {
                    _logger.LogError(item.Description);
                }
            }
            return View(role); // If something goes wrong, return the same view with the role model
        }

        // Details Action - Display details for a specific role
        public async Task<IActionResult> Details(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(); // If role is not found
            }
            return View(role); // Pass the role to the Details view
        }

        // Update Action - Display Update form for a specific role
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(); // If role is not found
            }

            // Create a ViewModel to display editable data
            var roleModel = new RoleUpdateViewModel
            {
                Id = role.Id,
                Name = role.Name
            };

            return View(roleModel); // Pass the ViewModel to the Update view
        }

        // Handle role update via Post request
        [HttpPost]
        public async Task<IActionResult> Update(RoleUpdateViewModel roleModel)
        {
            if (!ModelState.IsValid)
            {
                return View(roleModel); // If model is invalid, return the same view
            }

            var role = await _roleManager.FindByIdAsync(roleModel.Id);
            if (role == null)
            {
                return NotFound(); // If role is not found
            }

            role.Name = roleModel.Name; // Update role name
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role updated successfully");
                return RedirectToAction("Index");
            }

            // Log errors if update failed
            foreach (var item in result.Errors)
            {
                _logger.LogError(item.Description);
            }

            return View(roleModel); // Return the same view with the model if update fails
        }

        // Delete Action - Handle deletion of role
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(); // If role is not found
                }

                var result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Role deleted successfully");
                    return RedirectToAction("Index");
                }

                // Log errors if deletion failed
                foreach (var item in result.Errors)
                {
                    _logger.LogError(item.Description);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting role: {Message}", e.Message);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddOrRemoveUsers(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return NotFound();

            ViewBag.RoleId = roleId;
            var users = await _userManager.Users.ToListAsync(); // يفضل استخدام ToListAsync

            var usersInRole = new List<UserInRoleViewModel>();

            foreach (var user in users)
            {
                var userInRole = new UserInRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                    userInRole.IsSelected = true;
                else
                    userInRole.IsSelected = false;
                usersInRole.Add(userInRole);
            }

            return View(usersInRole);
        }
        [HttpPost]
        public async Task<IActionResult> AddOrRemoveUsers(string roleId, List<UserInRoleViewModel> users)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null)
            {
                return NotFound();
            }

            ViewBag.RoleId = roleId;
            if (ModelState.IsValid)
            {
                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.UserId);
                    if (appUser is not null)
                    {
                        if (user.IsSelected && !await _userManager.IsInRoleAsync(appUser, role.Name))
                        {
                            await _userManager.AddToRoleAsync(appUser, role.Name);
                        }
                        else if (!user.IsSelected && await _userManager.IsInRoleAsync(appUser, role.Name))
                        {
                            await _userManager.RemoveFromRoleAsync(appUser, role.Name);

                        }
                    }
                }
                return RedirectToAction("Update", new { Id = roleId });
            }
            return View(users);

        }
    }
}