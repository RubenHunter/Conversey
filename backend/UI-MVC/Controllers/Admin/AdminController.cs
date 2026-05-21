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
    public async Task<IActionResult> CreateWorkspaceAdmin(AdminFormViewModel<WorkspaceAdmin> workspaceAdminFormViewModel)
    {
        await adminManager.AddWorkspaceAdmin(workspaceAdminFormViewModel.FormItem.Email,
            workspaceAdminFormViewModel.FormItem.Workspace.Id);
        return RedirectToAction("WorkspaceDetails", "ConverseyAdmin",
            new { id = workspaceAdminFormViewModel.FormItem.Workspace.Id});
        
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
    public async Task<IActionResult> EditWorkspaceAdmin(Guid id, AdminFormViewModel<WorkspaceAdmin> workspaceAdminViewModel)
    {
        try
        {
            var workspaceAdmin = workspaceAdminViewModel.FormItem;
            await adminManager.EditWorkspaceAdmin(workspaceAdmin);

            return RedirectToAction("WorkspaceDetails", "ConverseyAdmin", new { id = workspaceAdmin.Workspace.Id });
        }
        catch (NotFoundException notFoundException)
        {
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ModelStateHelper.ApplyValidationException(ModelState, ex);
        }

        return View(EditFormVm(workspaceAdminViewModel.FormItem));
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
    
}