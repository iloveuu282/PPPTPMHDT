using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;

namespace TroLyQuyChe.Web.Pages.Account;
public class LoginModel(AdminQueries queries, BrowserOwner owners) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Nhập tài khoản.")] public string Username { get; set; } = "";
    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu."), DataType(DataType.Password)] public string Password { get; set; } = "";
    public async Task OnGetAsync() => await owners.GetAsync(HttpContext);
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await queries.UserAsync(Username.Trim().ToLowerInvariant());
        if (user == null || new PasswordHasher<AppUser>().VerifyHashedPassword(user, user.PasswordHash, Password) == PasswordVerificationResult.Failed)
        { ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng."); return Page(); }
        var owner = await owners.GetAsync(HttpContext);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, owner), new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role), new Claim("AppUserId", user.Id.ToString())], CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return RedirectToPage("/Admin/Index");
    }
}
