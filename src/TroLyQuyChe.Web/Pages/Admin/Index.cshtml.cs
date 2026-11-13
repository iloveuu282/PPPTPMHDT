using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;
namespace TroLyQuyChe.Web.Pages.Admin;
public class IndexModel(AdminQueries queries, VersionAdministrationService admin) : PageModel
{
    public List<Document> Documents { get; set; } = [];
    [BindProperty] public string Code { get; set; } = "";
    [BindProperty] public string Title { get; set; } = "";
    public async Task OnGetAsync() => Documents = await queries.DocumentsAsync();
    public async Task<IActionResult> OnPostAsync()
    {
        try { var id = await admin.CreateDocumentAsync(User, Code, Title); TempData["Message"] = "Đã tạo văn bản. Tiếp theo hãy tạo bản nháp."; return RedirectToPage(new { documentId = id }); }
        catch (BusinessException ex) { ModelState.AddModelError("", ex.Message); await OnGetAsync(); return Page(); }
    }
    public static string State(DocumentVersion v)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "SE Asia Standard Time").DateTime);
        return VersionPresentation.State(v, today);
    }
}
