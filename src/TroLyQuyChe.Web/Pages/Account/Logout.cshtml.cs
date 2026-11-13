using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Services;
namespace TroLyQuyChe.Web.Pages.Account;
public class LogoutModel(BrowserOwner owners) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        var owner = await owners.GetAsync(HttpContext);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, owner)], CookieAuthenticationDefaults.AuthenticationScheme)));
        return RedirectToPage("/Index");
    }
    public IActionResult OnGet() => RedirectToPage("/Index");
}
