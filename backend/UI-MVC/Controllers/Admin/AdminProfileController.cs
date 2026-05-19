using Conversey.UI_MVC.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Roles = "ConverseyAdmin,WorkspaceAdmin")]
public class AdminProfileController(UserManager<IdentityUser> userManager) : Controller
{
    [HttpGet("/admin/profile")]
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var model = new AdminProfileViewModel
        {
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber
        };

        return View(model);
    }

    [HttpPost("/admin/profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AdminProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!string.Equals(user.UserName, model.Username, StringComparison.Ordinal))
        {
            var usernameResult = await userManager.SetUserNameAsync(user, model.Username);
            if (!usernameResult.Succeeded)
            {
                foreach (var error in usernameResult.Errors)
                {
                    ModelState.AddModelError(nameof(model.Username), error.Description);
                }
            }
        }

        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await userManager.SetEmailAsync(user, model.Email);
            if (!emailResult.Succeeded)
            {
                foreach (var error in emailResult.Errors)
                {
                    ModelState.AddModelError(nameof(model.Email), error.Description);
                }
            }
        }

        if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.Ordinal))
        {
            var phoneResult = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            if (!phoneResult.Succeeded)
            {
                foreach (var error in phoneResult.Errors)
                {
                    ModelState.AddModelError(nameof(model.PhoneNumber), error.Description);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["ProfileSaved"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
