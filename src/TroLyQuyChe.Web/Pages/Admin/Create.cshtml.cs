using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Services;
namespace TroLyQuyChe.Web.Pages.Admin;
public class CreateModel(VersionAdministrationService admin, TroLyQuyChe.Web.Data.AdminQueries queries) : PageModel
{
    [BindProperty(SupportsGet = true)] public int DocumentId { get; set; }
    [BindProperty] public string Label { get; set; } = "";
    [BindProperty] public DateOnly From { get; set; } = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "SE Asia Standard Time").DateTime);
    [BindProperty] public DateOnly? To { get; set; }
    [BindProperty(SupportsGet = true)] public int? CopyFrom { get; set; }
    public async Task OnGetAsync()
    {
        if (!CopyFrom.HasValue)
            CopyFrom = (await queries.VersionsWithClausesAsync(DocumentId))
                .Where(v => v.Status == TroLyQuyChe.Web.Data.VersionStatus.Approved && v.EffectiveFrom <= From)
                .OrderByDescending(v => v.EffectiveFrom).ThenByDescending(v => v.Id).Select(v => (int?)v.Id).FirstOrDefault();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { var id = await admin.CreateDraftAsync(User, DocumentId, Label, From, To, CopyFrom); TempData["Message"] = "Đã tạo bản nháp. Nhập điều khoản rồi lưu trước khi duyệt."; return RedirectToPage("Edit", new { id }); }
        catch (BusinessException ex) { ModelState.AddModelError("", ex.Message); return Page(); }
    }
}
