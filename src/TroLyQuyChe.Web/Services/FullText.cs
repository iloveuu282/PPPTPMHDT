using System.Text.RegularExpressions;

namespace TroLyQuyChe.Web.Services;

/// <summary>Readable presentation only. Never rewrites stored questions, citations or approved clauses.</summary>
public static class FullText
{
    // Whole terms only, so IT does not change words such as Internet or digital.
    private static readonly IReadOnlyDictionary<string, string> Terms =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["WFH"] = "làm việc tại nhà",
            ["work from home"] = "làm việc tại nhà",
            ["remote"] = "làm việc từ xa",
            ["VPN"] = "mạng riêng ảo",
            ["IT"] = "công nghệ thông tin",
            ["CNTT"] = "công nghệ thông tin",
            ["HR"] = "nhân sự",
            ["ID"] = "mã định danh",
            ["laptop"] = "máy tính xách tay",
            ["wifi"] = "mạng không dây",
            ["wi-fi"] = "mạng không dây",
            ["Approved"] = "đã duyệt",
            ["Draft"] = "bản nháp",
            ["Revoked"] = "đã thu hồi"
        };
    private static readonly Regex TermPattern = new(
        @"(?<![\p{L}\p{N}_])(?:" + string.Join("|", Terms.Keys.Select(Regex.Escape)) + @")(?![\p{L}\p{N}_])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string Expand(string? text) => string.IsNullOrEmpty(text) ? text ?? ""
        : TermPattern.Replace(text, match => Terms[match.Value]);
    public static bool HasChanges(string? text) => text is not null && Expand(text) != text;
    public const string SourceNotice = "Thuật ngữ được viết đầy đủ khi hiển thị; nội dung nguồn đã lưu được giữ nguyên.";
}
