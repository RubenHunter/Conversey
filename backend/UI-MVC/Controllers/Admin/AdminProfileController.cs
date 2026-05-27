using Conversey.BL.Administration;
using Conversey.UI_MVC.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Conversey.DAL;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Policy = AdminPolicy.Name)]
public class AdminProfileController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IAdminManager adminManager)
    : Controller
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

    [HttpPost("/admin/profile/change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordInputModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray())
            });
        }

        if (!string.IsNullOrWhiteSpace(model.CurrentPassword))
        {
            var changePasswordResult = await userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Password change failed.",
                    errors = changePasswordResult.Errors.Select(error => error.Description).ToArray()
                });
            }
        }
        else
        {
            var removeResult = await userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Failed to remove current password.",
                    errors = removeResult.Errors.Select(error => error.Description).ToArray()
                });
            }

            var addResult = await userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Password change failed.",
                    errors = addResult.Errors.Select(error => error.Description).ToArray()
                });
            }
        }

        if (user is WorkspaceAdminUser workspaceAdmin)
        {
            await adminManager.SetWorkspaceAdminFirstLogin(Guid.Parse(workspaceAdmin.Id), false);
        }
        else if (user is ConverseyAdminUser converseyAdmin)
        {
            await adminManager.SetConverseyAdminFirstLogin(Guid.Parse(converseyAdmin.Id), false);
        }

        await signInManager.RefreshSignInAsync(user);
        return Ok(new { message = "Password updated." });
    }
}
