using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
namespace TroLyQuyChe.Web.Pages.Admin;
public class HistoryModel(AdminQueries queries) : PageModel
{
    public List<QaEntry> Entries { get; set; } = [];
    public async Task OnGetAsync() => Entries = await queries.HistoryAsync();
}
