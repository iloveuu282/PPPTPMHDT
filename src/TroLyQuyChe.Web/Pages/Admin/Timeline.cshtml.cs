using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
namespace TroLyQuyChe.Web.Pages.Admin;

public class TimelineModel(AdminQueries queries, TimeProvider clock) : PageModel
{
    public Document Document { get; private set; } = null!;
    public DateOnly Today { get; private set; }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var doc = (await queries.DocumentsAsync()).SingleOrDefault(d => d.Id == id);
        if (doc is null) return NotFound();
        Document = doc;
        Today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(clock.GetUtcNow(), "SE Asia Standard Time").DateTime);
        return Page();
    }
}
