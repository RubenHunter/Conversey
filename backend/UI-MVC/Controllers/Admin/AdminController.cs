using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult CreateWorkspaceAdmin(AdminFormViewModel<WorkspaceAdmin> workspaceAdminFormViewModel)
    {
        adminManager.AddWorkspaceAdmin(workspaceAdminFormViewModel.FormItem.Email,
            workspaceAdminFormViewModel.FormItem.Workspace.Id);
        return RedirectToAction("WorkspaceDetails", "ConverseyAdmin",
            new { id = workspaceAdminFormViewModel.FormItem.Workspace.Id});
        
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
                        ModelState.AddModelError($"WorkspaceAdmin.{member}", message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                }
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
    }
}