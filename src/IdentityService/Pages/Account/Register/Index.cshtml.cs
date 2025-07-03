using Duende.IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace IdentityService.Pages.Account.Register;

[SecurityHeaders]
[AllowAnonymous]
public class Index(UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public RegisterViewModel Input { get; set; } = default!;

    [BindProperty]
    public bool RegisterSuccess { get; set; }

    public IActionResult OnGet(string? url)
    {
        Input = new RegisterViewModel
        {
            ReturnUrl = url
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Input.Button != "register")
        {
            return Redirect("~/");
        }

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Username,
                Email = Input.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await userManager.AddClaimsAsync(user,
                [
                    new Claim(JwtClaimTypes.Name, Input.FullName)
                ]);

                RegisterSuccess = true;
            }
        }

        return Page();
    }
}
