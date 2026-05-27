using Conversey.BL.Domain.Common;
using Conversey.BL.Services;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Contacts;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class ContactsController : Controller
{
    private readonly IContactManager _contactManager;
    private readonly IEmailService _emailService;
    private readonly WorkspaceContext _workspaceContext;

    public ContactsController(IContactManager contactManager, IEmailService emailService, WorkspaceContext workspaceContext)
    {
        _contactManager = contactManager;
        _emailService = emailService;
        _workspaceContext = workspaceContext;
    }

    [HttpGet("/admin/contacts")]
    public IActionResult Index([FromQuery] string? projectId)
    {
        var workspaceId = _workspaceContext.CurrentWorkspace!.Id;
        Slug? projectFilter = string.IsNullOrWhiteSpace(projectId) ? null : new Slug { Text = projectId };

        var contacts = _contactManager.GetContactsByWorkspaceId(workspaceId, projectFilter);

        return View(new ContactsViewModel
        {
            Contacts = contacts,
            SelectedProjectId = projectId
        });
    }

    [HttpPost("/admin/contacts/send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendEmail([FromBody] SendContactEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new { error = "Email, subject, and body are required." });
        }

        try
        {
            await _emailService.SendEmailAsync(request.ToEmail, request.Subject, request.Body);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
