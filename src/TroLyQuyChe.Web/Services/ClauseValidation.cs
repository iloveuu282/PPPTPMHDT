using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services.Understanding;

namespace TroLyQuyChe.Web.Services;

public static class ClauseValidation
{
    // Days are positive integers. A lower bound is exclusive and an upper bound inclusive.
    // Return an explanation instead of guessing when persisted rules conflict or leave a gap.
    public static string? NoticeProblem(IEnumerable<Clause> source)
    {
        var rules = source.Where(c => c.Topic == "LeaveNotice")
            .OrderBy(c => c.MinimumLeaveDaysExclusive ?? 0).ToList();
        if (rules.Count == 0) return null;
        decimal? previousEnd = 0;
        foreach (var rule in rules)
        {
            var start = rule.MinimumLeaveDaysExclusive ?? 0;
            var end = rule.MaximumLeaveDaysInclusive;
            if (start < 0 || start > LeaveDurationParser.MaximumDays || start != decimal.Truncate(start)
                || (end.HasValue && (end <= start || end > LeaveDurationParser.MaximumDays || end != decimal.Truncate(end.Value))))
                return "Điều kiện báo trước cần cận nguyên ngày hợp lệ trong khoảng 0–99999.";
            if (previousEnd is null || start < previousEnd)
                return "Các trường hợp báo trước bị chồng nhau. Hãy sửa cận ngày nghỉ trước khi duyệt.";
            if (start > previousEnd)
                return "Các trường hợp báo trước còn khoảng ngày chưa được quy định. Hãy bổ sung trước khi duyệt.";
            previousEnd = end;
        }
        return previousEnd.HasValue ? "Trường hợp báo trước cuối cần để trống cận trên để bao phủ số ngày nghỉ lớn hơn." : null;
    }

    public static string[] MissingTopics(DocumentVersion next, IEnumerable<DocumentVersion> existing)
    {
        var required = PolicyCatalog.Bindings.Values.Where(b => b.DocumentCode == next.Document.Code).Select(b => b.ClauseTopic)
            .Concat(existing.Where(v => v.Status == VersionStatus.Approved).SelectMany(v => v.Clauses).Select(c => c.Topic)).Distinct();
        return required.Except(next.Clauses.Select(c => c.Topic)).Select(PolicyCatalog.TopicLabel).ToArray();
    }
}
