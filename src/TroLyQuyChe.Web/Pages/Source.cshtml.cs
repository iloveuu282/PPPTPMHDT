using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;

namespace TroLyQuyChe.Web.Pages;

public class SourceModel(CitationSourceService sources, BrowserOwner owners) : PageModel
{
    public Citation Citation { get; set; } = null!;
    public async Task<IActionResult> OnGetAsync(long id)
    {
        var found = await sources.FindAsync(id, await owners.GetAsync(HttpContext), User.IsInRole("Admin"));
        if (found is null) return NotFound("Không tìm thấy trích dẫn hoặc bạn không có quyền xem.");
        Citation = found;
        return Page();
    }
}
