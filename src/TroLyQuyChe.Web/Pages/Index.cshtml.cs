using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;

namespace TroLyQuyChe.Web.Pages;

public class IndexModel(QuestionAnswerService questions, HistoryService history, BrowserOwner owners, ILogger<IndexModel> logger) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Vui lòng nhập câu hỏi."), StringLength(2000, ErrorMessage = "Câu hỏi tối đa 2000 ký tự.")]
    public string Question { get; set; } = "";
    [BindProperty, DataType(DataType.Date)] public DateOnly? ApplicableDate { get; set; }
    public QaEntry? Result { get; set; }
    public List<QaEntry> Recent { get; set; } = [];
    public string? TechnicalError { get; set; }
    public static string StatusLabel(AnswerStatus s) => s switch
    {
        AnswerStatus.Answered => "Có căn cứ", AnswerStatus.NeedsClarification => "Cần làm rõ", _ => "Chưa đủ căn cứ"
    };
    public async Task<IActionResult> OnGetAsync(long? id)
    {
        var owner = await owners.GetAsync(HttpContext);
        try
        {
            Recent = await history.RecentAsync(owner);
            if (id.HasValue)
            {
                Result = await history.FindAsync(id.Value, owner);
                if (Result is null) return NotFound();
            }
        }
        catch (Exception ex) when (ex is System.Data.Common.DbException or InvalidOperationException)
        {
            logger.LogError(ex, "Không tải được lịch sử hỏi đáp");
            TechnicalError = "Không thể tải dữ liệu. Vui lòng thử lại hoặc liên hệ quản trị viên.";
        }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var owner = await owners.GetAsync(HttpContext);
        try
        {
            if (!ModelState.IsValid)
            {
                Recent = await history.RecentAsync(owner);
                return Page();
            }
            var id = await questions.AskAsync(Question, ApplicableDate, owner);
            return RedirectToPage("/Index", null, new { id }, "ket-qua");
        }
        catch (Exception ex) when (ex is System.Data.Common.DbException or Microsoft.EntityFrameworkCore.DbUpdateException or InvalidOperationException)
        {
            logger.LogError(ex, "Không lưu được lượt hỏi");
            TechnicalError = "Không thể xử lý và lưu lượt hỏi. Vui lòng thử lại hoặc liên hệ quản trị viên.";
            return Page();
        }
    }
}
