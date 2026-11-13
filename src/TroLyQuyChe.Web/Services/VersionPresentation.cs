using TroLyQuyChe.Web.Data;

namespace TroLyQuyChe.Web.Services;

public static class VersionPresentation
{
    public static string Period(DocumentVersion v) => v.ReplacedOnStartDate
        ? $"Đã được thay thế trong ngày {v.EffectiveFrom:dd/MM/yyyy}; chỉ giữ nguồn lịch sử"
        : $"{v.EffectiveFrom:dd/MM/yyyy} — {(v.ApplicableUntil.HasValue ? v.ApplicableUntil.Value.ToString("dd/MM/yyyy") : "Chưa giới hạn")}";
    public static string StoredState(VersionStatus status) => status switch
    {
        VersionStatus.Draft => "Bản nháp", VersionStatus.Revoked => "Đã thu hồi", _ => "Đã duyệt"
    };
    public static string State(DocumentVersion v, DateOnly day)
    {
        if (v.Status != VersionStatus.Approved) return StoredState(v.Status);
        if (v.EffectiveFrom > day) return "Đã duyệt · Chưa đến hiệu lực";
        if (v.Replacements.Any(n => n.Status != VersionStatus.Draft && n.EffectiveFrom <= day))
            return "Đã được thay thế";
        return v.ApplicableUntil < day ? "Đã duyệt · Hết hiệu lực" : "Đã duyệt · Đang hiệu lực";
    }
    public static string ActionLabel(string action) => action switch
    {
        "Created" => "Tạo bản nháp", "Edited" => "Lưu bản nháp", "Approved" => "Duyệt",
        "Revoked" => "Thu hồi", "Superseded" => "Ghi nhận thay thế", _ => action
    };
}
