using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Policy = ConverseyAdminPolicy.Name)]
public class AdminController(IAdminManager adminManager, IWorkspaceManager workspaceManager) : Controller
{
    
    [HttpGet("/admin/workspaces/admins/new")]
    public IActionResult CreateWorkspaceAdmin(Slug workspaceId)
    {
        var workspace = workspaceManager.GetWorkspaceById(workspaceId);
        return View(CreateFormVm(new WorkspaceAdmin { Workspace = workspace}));
    }
    
    [HttpPost("/admin/workspaces/admins/new")]
    public async Task<IActionResult> CreateWorkspaceAdmin(
        [FromForm(Name = "FormItem.Email")] string email,
        [FromForm(Name = "FormItem.Username")] string username,
        [FromForm(Name = "FormItem.PhoneNumber")] string phoneNumber,
        [FromForm] string workspaceId)
    {
        var workspaceSlug = new Slug { Text = workspaceId };

        try
        {
            var (admin, oneTimePassword) = await adminManager.AddWorkspaceAdmin(email, username, phoneNumber, workspaceSlug);

            TempData["OneTimePassword"] = oneTimePassword;
            TempData["OneTimePasswordAdminEmail"] = admin.Email;
            TempData["OneTimePasswordWorkspaceId"] = workspaceSlug.Text;

            var redirectUrl = Url.Action("WorkspaceDetails", "ConverseyAdmin", new { id = workspaceSlug });

            if (IsAjaxRequest())
            {
                return Ok(new { redirectUrl });
            }

            return RedirectToAction("WorkspaceDetails", "ConverseyAdmin", new { id = workspaceSlug });
        }
        catch (ValidationException ex)
        {
            ApplyValidationExceptionToModelState(ex);
        }

        if (IsAjaxRequest())
        {
            return BadRequest(new { errors = ToErrorPayload(ModelState) });
        }

        return View(CreateFormVm(new WorkspaceAdmin
        {
            Email = email,
            Username = username,
            PhoneNumber = phoneNumber,
            Workspace = workspaceManager.GetWorkspaceById(workspaceSlug)
        }));
    }
    
    [HttpGet("/admin/workspace/admin/{id}")]
    public async Task<IActionResult> EditWorkspaceAdmin(Guid id)
    {
        try
        {
            var workspaceAdmin = await adminManager.GetWorkspaceAdminById(id);

            return View(EditFormVm(workspaceAdmin));
        }
        catch (NotFoundException e)
        {
            return BadRequest(e);
        }
    }

    [HttpPost("/admin/workspace/admin/{id}")]
    public async Task<IActionResult> EditWorkspaceAdmin(
        Guid id,
        [FromForm(Name = "FormItem.Email")] string email,
        [FromForm(Name = "FormItem.Username")] string username,
        [FromForm(Name = "FormItem.PhoneNumber")] string phoneNumber,
        [FromForm] string workspaceId)
    {
        var workspaceSlug = new Slug { Text = workspaceId };
        var workspaceAdmin = new WorkspaceAdmin
        {
            Id = id,
            Email = email,
            Username = username,
            PhoneNumber = phoneNumber,
            Workspace = workspaceManager.GetWorkspaceById(workspaceSlug)
        };

        try
        {
            await adminManager.EditWorkspaceAdmin(workspaceAdmin);

            if (IsAjaxRequest())
            {
                var redirectUrl = Url.Action("WorkspaceDetails", "ConverseyAdmin", new { id = workspaceAdmin.Workspace.Id });
                return Ok(new { redirectUrl });
            }

            return RedirectToAction("WorkspaceDetails", "ConverseyAdmin", new { id = workspaceAdmin.Workspace.Id });
        }
        catch (NotFoundException notFoundException)
        {
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ApplyValidationExceptionToModelState(ex);
        }

        if (IsAjaxRequest())
        {
            return BadRequest(new { errors = ToErrorPayload(ModelState) });
        }

        return View(EditFormVm(workspaceAdmin));
    }
    
    [HttpPost("/admin/workspace-user/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkspaceAdmin(Guid id)
    {
        try
        {
            var workspaceAdmin = await adminManager.GetWorkspaceAdminById(id);
            var workspace = workspaceAdmin.Workspace;
            await adminManager.RemoveWorkspaceAdmin(id);
            return RedirectToAction("WorkspaceDetails", "ConverseyAdmin", workspace);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    private AdminFormViewModel<WorkspaceAdmin> CreateFormVm(WorkspaceAdmin workspaceAdmin)
    {
        return new AdminFormViewModel<WorkspaceAdmin>
        {
            FormItem = workspaceAdmin,
            FormAction = "CreateWorkspaceAdmin",
            SubmitLabel = "Create WorkspaceAdmin",
        };
    }
    
    private AdminFormViewModel<WorkspaceAdmin> EditFormVm(WorkspaceAdmin workspaceAdmin)
    {
        return new AdminFormViewModel<WorkspaceAdmin>
        {
            FormItem = workspaceAdmin,
            FormAction = "EditWorkspaceAdmin",
            SubmitLabel = "Edit WorkspaceAdmin",
        };
    }
    
    private void ApplyValidationExceptionToModelState(ValidationException ex)
    {
        if (ex.Data["ValidationResults"] is List<ValidationResult> results)
        {
            foreach (var result in results)
            {
                var message = result.ErrorMessage ?? "Invalid value";

                if (result.MemberNames.Any())
                {
                    foreach (var member in result.MemberNames)
                    {
                        ModelState.AddModelError($"FormItem.{member}", message);
                    }
                }
                else
                {
                    ModelState.AddModelError("FormError", message);
                }
            }
        }
        else
        {
            ModelState.AddModelError("FormError", ex.Message);
        }
    }

    private bool IsAjaxRequest()
    {
        return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string[]> ToErrorPayload(ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .ToArray());
    }
}
