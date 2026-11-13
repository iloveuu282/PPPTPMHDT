using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TroLyQuyChe.Web.Services;

public class BrowserOwner
{
    public async Task<string> GetAsync(HttpContext context)
    {
        var owner = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (owner is not null) return owner;
        owner = Guid.NewGuid().ToString("N");
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, owner)], CookieAuthenticationDefaults.AuthenticationScheme));
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        // Razor generates the antiforgery token later in this same request.
        context.User = principal;
        return owner;
    }
}
