// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Conversey.DAL;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Conversey.UI_MVC.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly WorkspaceContext _workspaceContext;
        private readonly ILogger<LoginModel> _logger;
        private readonly AdminContext _adminContext;
        private readonly ConverseyDbContext _dbContext;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            WorkspaceContext workspaceContext,
            ILogger<LoginModel> logger,
            AdminContext adminContext,
            ConverseyDbContext dbContext
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _workspaceContext = workspaceContext;
            _logger = logger;
            _adminContext = adminContext;
            _dbContext = dbContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }
        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                // FindByEmailAsync does not eager-load navigation properties.
                // Reload WorkspaceAdminUser with Workspace included so the workspace check below works.
                if (user is WorkspaceAdminUser)
                {
                    user = await _dbContext.WorkspaceAdmins
                        .Include(w => w.Workspace)
                        .FirstOrDefaultAsync(w => w.Email == Input.Email)
                        ?? user;
                }

                // Check if user is a ConverseyAdmin
                if (user is ConverseyAdminUser converseyAdmin)
                {
                    if (_workspaceContext.CurrentWorkspace == null)
                    {
                        var result = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, Input.RememberMe);
                            _logger.LogInformation("ConverseyAdmin logged in.");
                            _adminContext.CurrentAdmin = AdminContext.ToDomain(converseyAdmin);
                            return LocalRedirect(Url.Content("~/admin"));
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Workspace mismatch for login attempt.");
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                    

                }

                // Check if user is a WorkspaceAdmin
                if (user is WorkspaceAdminUser workspaceAdmin)
                {
                    var workspace = _workspaceContext.CurrentWorkspace;

                    // On the root domain: validate credentials and redirect to the correct subdomain
                    if (workspace == null)
                    {
                        var result = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            var workspaceSlug = workspaceAdmin.Workspace?.Id.Text;
                            if (string.IsNullOrWhiteSpace(workspaceSlug))
                            {
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                return Page();
                            }

                            // Sign in so the cookie is set, then redirect to the subdomain
                            await _signInManager.SignInAsync(user, Input.RememberMe);
                            var host = HttpContext.Request.Host.Host;
                            var subdomainUrl = $"{HttpContext.Request.Scheme}://{workspaceSlug}.{host}/admin/workspace";
                            return Redirect(subdomainUrl);
                        }

                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }

                    // On a workspace subdomain: enforce workspace match
                    if (workspaceAdmin.Workspace == null ||
                        string.IsNullOrWhiteSpace(workspaceAdmin.Workspace.Id.Text) ||
                        workspaceAdmin.Workspace.Id != workspace.Id)
                    {
                        _logger.LogWarning("Workspace mismatch for login attempt.");
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }

                    var signInResult = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
                    if (signInResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, Input.RememberMe);
                        _logger.LogInformation("WorkspaceAdmin logged in.");
                        _adminContext.CurrentAdmin = AdminContext.ToDomain(workspaceAdmin);
                        if (workspaceAdmin.FirstLogin)
                        {
                            TempData["ForcePasswordChange"] = true;
                        }
                        return LocalRedirect(Url.Content("~/admin/workspace"));
                    }
                }
                
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
