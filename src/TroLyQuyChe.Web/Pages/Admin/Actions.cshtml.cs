using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
namespace TroLyQuyChe.Web.Pages.Admin;
public class ActionsModel(AdminQueries queries) : PageModel
{
    public List<VersionAction> Actions { get; set; } = [];
    public async Task OnGetAsync(int? version) => Actions = await queries.ActionsAsync(version);
}
