using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;
namespace TroLyQuyChe.Web.Pages.Admin;
public class ClauseForm
{
    public string? Article { get; set; }
    public string? Topic { get; set; }
    public string? Body { get; set; }
    public string? Location { get; set; }
    public decimal? Minimum { get; set; }
    public decimal? Maximum { get; set; }
    public bool Remove { get; set; }
}
public class EditModel(AdminQueries queries, VersionAdministrationService admin,
    Microsoft.Extensions.Options.IOptions<ApplicationIdentity> identity) : PageModel
{
    public DocumentVersion Version { get; set; } = null!;
    [BindProperty] public string Revision { get; set; } = "";
    [BindProperty] public string? Label { get; set; }
    [BindProperty] public DateOnly From { get; set; }
    [BindProperty] public DateOnly? To { get; set; }
    [BindProperty] public List<ClauseForm> Clauses { get; set; } = [];
    [BindProperty] public string? Reason { get; set; }
    [BindProperty] public bool EffectiveOnApproval { get; set; }
    [BindProperty] public bool ReplaceCurrent { get; set; }
    [BindProperty] public bool AcknowledgeMissingTopics { get; set; }
    public string[] MissingTopics { get; set; } = [];
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var found = await queries.VersionAsync(id); if (found == null) return NotFound(); Version = found;
        Revision = Convert.ToBase64String(Version.RowVersion); Label = Version.VersionLabel; From = Version.EffectiveFrom; To = Version.EffectiveTo;
        Clauses = Version.Clauses.OrderBy(c => c.Id).Select(c => new ClauseForm { Article = c.ArticleNumber, Topic = c.Topic, Body = FullText.Expand(c.Body), Location = identity.Value.SourceLabel(c.SourceLocation), Minimum = c.MinimumLeaveDaysExclusive, Maximum = c.MaximumLeaveDaysInclusive }).ToList();
        EffectiveOnApproval = true; Clauses.Add(new()); await LoadWarnings(); return Page();
    }
    public Task<IActionResult> OnPostSaveAsync(int id) => Execute(id, async revision => await admin.SaveDraftAsync(User, id, revision, Label ?? "", From, To,
        Clauses.Where(c => !c.Remove && (!string.IsNullOrWhiteSpace(c.Article) || !string.IsNullOrWhiteSpace(c.Body))).Select(c => new ClauseInput(c.Article ?? "", c.Topic ?? "", c.Body ?? "", c.Location ?? "", c.Minimum, c.Maximum)).ToList()));
    public Task<IActionResult> OnPostApproveAsync(int id) => Execute(id, revision => admin.ApproveAsync(User, id, revision, ReplaceCurrent, EffectiveOnApproval, AcknowledgeMissingTopics));
    public Task<IActionResult> OnPostRevokeAsync(int id) => Execute(id, revision => admin.RevokeAsync(User, id, revision, Reason ?? ""));
    public async Task<IActionResult> OnPostAddRowAsync(int id)
    {
        Version = (await queries.VersionAsync(id))!; if (Version == null) return NotFound(); Clauses.Add(new()); await LoadWarnings(); return Page();
    }
    private async Task<IActionResult> Execute(int id, Func<byte[], Task> action)
    {
        try
        {
            if (!ModelState.IsValid) throw new BusinessException("Dữ liệu nhập chưa hợp lệ. Kiểm tra ngày và các trường điều khoản.");
            await action(Convert.FromBase64String(Revision)); TempData["Message"] = "Thao tác thành công, đã ghi nhật ký."; return RedirectToPage(new { id });
        }
        catch (Exception ex) when (ex is BusinessException or FormatException or Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            var error = ex is BusinessException ? ex.Message : "Dữ liệu đã thay đổi hoặc mã phiên bản không hợp lệ. Hãy tải lại trang.";
            if (Clauses.Count == 0) { ModelState.Clear(); if (await OnGetAsync(id) is NotFoundResult) return NotFound(); }
            else { Version = (await queries.VersionAsync(id))!; if (Version == null) return NotFound(); }
            await LoadWarnings(); ModelState.AddModelError("", error); return Page();
        }
    }
    private async Task LoadWarnings() => MissingTopics = ClauseValidation.MissingTopics(Version, await queries.VersionsWithClausesAsync(Version.DocumentId));
}
