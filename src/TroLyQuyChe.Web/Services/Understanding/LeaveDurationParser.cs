using System.Globalization;
using System.Text.RegularExpressions;

namespace TroLyQuyChe.Web.Services.Understanding;

public record LeaveDuration(decimal? From = null, decimal? To = null, bool Unbounded = false, string? Error = null);

// Parse requested working days, never the notice period or a policy limit.
public static class LeaveDurationParser
{
    public const decimal MaximumDays = 99999;
    public static LeaveDuration Parse(string normalized)
    {
        const string error = "Vui lòng nêu một số nguyên ngày làm việc dự định nghỉ từ 1 đến 99999, hoặc bỏ thời lượng để xem các trường hợp báo trước.";
        var q = Regex.Replace(normalized, @"\b(?:xin vang co phep|vang co phep|xin nghi phep|nghi phep|xin nghi)\b", "nghi");
        foreach (var (word, value) in QuestionVocabulary.LeaveDurationWords)
            q = Regex.Replace(q, @"(\bnghi\s+(?:trong\s+|tu\s+)?)" + word + @"(?=\s+(?:ngay|hom)\b)", m => m.Groups[1].Value + value.ToString(CultureInfo.InvariantCulture));
        if (Regex.IsMatch(q, @"\bnghi\s+(?:-\s*\d|nua|\d+\s*(?:tuan|thang))\b")) return new(Error: error);
        var above = Regex.Match(q, @"\bnghi\s+(?:tren|hon)\s+(\d+)\s+(?:ngay|hom)\b");
        if (above.Success)
        {
            if (!decimal.TryParse(above.Groups[1].Value, out var lower) || lower < 0 || lower >= MaximumDays) return new(Error: error);
            return new(lower + 1, Unbounded: true);
        }
        var numbers = Regex.Matches(q, @"\bnghi\s+(?:(?:trong|tu)\s+)?(\d+(?:[.,]\d+)?)\s*(?:ngay|hom)\b");
        if (numbers.Count == 0) return new();
        var values = new List<decimal>();
        foreach (Match m in numbers)
        {
            if (!decimal.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
                || value < 1 || value > MaximumDays || value != decimal.Truncate(value)) return new(Error: error);
            values.Add(value);
        }
        if (values.Distinct().Count() != 1) return new(Error: "Bạn vui lòng nêu một thời lượng nghỉ, hoặc hỏi riêng từng trường hợp.");
        var range = Regex.IsMatch(q, @"\bnghi\s+tu\s+\d+\s+(?:ngay|hom)(?:\s+lam viec)?\s+(?:tro len|ve sau)\b");
        return new(values[0], Unbounded: range);
    }
}
