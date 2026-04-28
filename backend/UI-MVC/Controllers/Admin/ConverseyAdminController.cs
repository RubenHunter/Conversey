using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL;
using Conversey.DAL.Administration;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Policy = ConverseyAdminPolicy.Name)]
public class ConverseyAdminController(IWorkspaceManager workspaceManager, IAdminManager adminManager) : Controller
{
    [HttpGet("/admin/conversey")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("admin/workspaces")]
    public IActionResult Workspaces()
    {
        var workspaces = workspaceManager.GetAllWorkspaces();
        return View(workspaces);
    }

    [HttpGet("/admin/workspaces/new")]
    public IActionResult CreateWorkspace()
    {
        return View(CreateFormVm(new Workspace()));
    }
    
    [HttpPost("/admin/workspaces/new")]
    public IActionResult CreateWorkspace(AdminFormViewModel<Workspace> workspaceFormViewModel)
    {
        workspaceManager.AddWorkspace(workspaceFormViewModel.FormItem.Name);
        return RedirectToAction("Workspaces");
    }

    [HttpGet("/admin/workspaces/{id}")]
    public IActionResult WorkspaceDetails(Slug id)
    {
        try
        {
            var workspace = workspaceManager.GetWorkspaceById(id);
            var workspaceAdmins = adminManager.GetAllWorkspaceAdminsByWorkspaceIdWithWorkspace(id);
            
            return View(new AdminWorkspaceDetailsViewModel
            {                           
                Workspace = workspace,
                WorkspaceAdmins = workspaceAdmins
            });
        }
        catch (NotFoundException e)
        {
            //TODO 404 Page
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("/admin/workspace/{id}")]
    public IActionResult EditWorkspace(Slug id)
    {
        try
        {
            var workspace = workspaceManager.GetWorkspaceById(id);

            return View(EditFormVm(workspace));
        }
        catch (NotFoundException e)
        {
            return BadRequest(e);
        }
    }
    
    [HttpPost("/admin/workspace/{id}")]
    public IActionResult EditWorkspace(Slug id, AdminFormViewModel<Workspace> workspaceFormViewModel)
    {
        try
        {
            var workspace = workspaceFormViewModel.FormItem;
            workspace.Id = id;
            workspaceManager.EditWorkspace(workspace);

            return RedirectToAction("Workspaces");
        }
        catch (NotFoundException notFoundException)
        {
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ApplyValidationExceptionToModelState(ex);
        }
        
        return View(EditFormVm(workspaceFormViewModel.FormItem));
    }
    
    [HttpPost("/admin/workspace/delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteWorkspace(Slug id)
    {
        try
        {
            workspaceManager.RemoveWorkspace(id);
            return RedirectToAction("Workspaces");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    
    private AdminFormViewModel<Workspace> CreateFormVm(Workspace workspace)
    {
        return new AdminFormViewModel<Workspace>
        {
            FormItem = workspace,
            FormAction = "CreateWorkspace",
            SubmitLabel = "Create Workspace",
        };
    }
    
    private AdminFormViewModel<Workspace> EditFormVm(Workspace workspace)
    {
        return new AdminFormViewModel<Workspace>
        {
            FormItem = workspace,
            FormAction = "EditWorkspace",
            SubmitLabel = "Edit Workspace",
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
                        ModelState.AddModelError($"Workspace.{member}", message);
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