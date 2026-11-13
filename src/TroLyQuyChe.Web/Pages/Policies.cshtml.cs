using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TroLyQuyChe.Web.Data;

namespace TroLyQuyChe.Web.Pages;

public class PoliciesModel(RegulationDbContext db, TimeProvider clock) : PageModel
{
    public DateOnly Today { get; private set; }
    public List<DocumentVersion> Versions { get; private set; } = [];
    public async Task OnGetAsync()
    {
        Today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(clock.GetUtcNow(), "SE Asia Standard Time").DateTime);
        var candidates = await db.DocumentVersions.AsNoTracking()
            .Where(v => v.Status == VersionStatus.Approved && v.EffectiveFrom <= Today)
            .Include(v => v.Document).Include(v => v.Clauses).Include(v => v.Replacements)
            .OrderBy(v => v.Document.Code).ToListAsync();
        Versions = candidates.Where(v => v.ApplicableUntil == null || v.ApplicableUntil >= Today).ToList();
    }
}
