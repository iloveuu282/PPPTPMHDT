using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TroLyQuyChe.Web.Services.Understanding;

namespace TroLyQuyChe.Web.Services;

public record DateResolution(DateOnly? From, DateOnly? To, string? Clarification);

public class ApplicableDateResolver(TimeProvider clock)
{
    public static string Normalize(string text) => new(text.ToLowerInvariant().Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        .Select(c => c == 'đ' ? 'd' : c).ToArray());

    public DateResolution Resolve(string question, DateOnly? selected)
    {
        var q = Regex.Replace(Normalize(question), @"\s+", " ").Trim();
        // “Bao nhiêu ngày 1 tuần” describes frequency, not an incomplete calendar date.
        q = Regex.Replace(q, @"\b(?:1|mot)\s+tuan\b", "moi tuan");
        var ranges = new List<(DateOnly From, DateOnly To)>();
        var invalid = false;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).DateTime);
        // Read full dates first so the month suffix of dd/MM/yyyy is not treated as a second date.
        q = Regex.Replace(q, @"(?<!\d)\d{4}-\d{1,2}-\d{1,2}(?!\d)", m =>
        {
            if (DateOnly.TryParseExact(m.Value, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)) ranges.Add((day, day));
            else invalid = true;
            return " ";
        });
        q = Regex.Replace(q, @"(?<!\d)(\d{1,2})[/-](\d{1,2})[/-](\d{4})(?!\d)", m =>
        {
            if (DateOnly.TryParseExact(m.Value.Replace('-', '/'), "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)) ranges.Add((day, day));
            else invalid = true;
            return " ";
        });
        q = Regex.Replace(q, @"\b(hom nay|hom qua|ngay mai)\b", m =>
        {
            var day = today.AddDays(m.Value == "hom qua" ? -1 : m.Value == "ngay mai" ? 1 : 0);
            ranges.Add((day, day));
            return " ";
        });
        q = Regex.Replace(q, @"(?:thang\s*)?(?<!\d)(\d{1,2})\s*[/.-]\s*(\d{4})(?!\d)|thang\s+(\d{1,2})\s+nam\s+(\d{4})", m =>
        {
            var month = int.Parse(m.Groups[m.Groups[1].Success ? 1 : 3].Value);
            var year = int.Parse(m.Groups[m.Groups[2].Success ? 2 : 4].Value);
            if (month is >= 1 and <= 12 && year is >= 1 and <= 9999)
                ranges.Add((new(year, month, 1), new(year, month, DateTime.DaysInMonth(year, month))));
            else invalid = true;
            return " ";
        });
        q = Regex.Replace(q, @"\bnam\s+(\d{4})\b", m =>
        {
            var year = int.Parse(m.Groups[1].Value);
            if (year == 0) invalid = true;
            else ranges.Add((new(year, 1, 1), new(year, 12, 31)));
            return " ";
        });
        if (invalid) return new(selected, selected, "Mốc thời gian không hợp lệ. Vui lòng nhập ngày theo dd/MM/yyyy hoặc tháng/năm.");
        if (selected.HasValue)
        {
            if (ranges.Any(r => selected < r.From || selected > r.To))
                return new(selected, selected, "Ngày áp dụng đã chọn mâu thuẫn với mốc trong câu hỏi. Vui lòng thống nhất ngày cần tra cứu.");
            return new(selected, selected, null);
        }
        if (ranges.Distinct().Count() > 1)
            return new(null, null, "Câu hỏi có nhiều mốc thời gian. Vui lòng chọn một ngày áp dụng và chỉ giữ mốc tương ứng trong câu hỏi.");
        if (ranges.Count > 0) return new(ranges[0].From, ranges[0].To, null);
        // 'Chuyển phép sang năm sau' hỏi quy tắc định kỳ, không yêu cầu tra một năm tương lai cụ thể.
        if (Regex.IsMatch(q, @"\b(?:phep nam|phep ton|chuyen phep)\b") && Regex.IsMatch(q, @"\b(?:chuyen|chua dung|chua su dung)\b"))
            q = Regex.Replace(q, @"\b(?:nam sau|nam ke tiep)\b", " ");
        if (TemporalVocabulary.UnspecifiedHistoricalPatterns.Any(pattern => Regex.IsMatch(q, pattern)))
            return new(null, null, "Bạn muốn tra quy chế vào ngày hoặc tháng/năm nào trong quá khứ?");
        q = Regex.Replace(q, @"\b(?:(?:moi|mot|1)\s+thang|thang\s+(?:duoc|co the|toi da|nghi|tinh|duong lich)|(?:trong|theo)\s+thang)\b", " ");
        if (Regex.IsMatch(q, @"\b(?:nghi|phep)\b") && Regex.IsMatch(q, @"\b(?:bao nhieu|may|toi da|han muc|gioi han)\b"))
            q = Regex.Replace(q, @"\bthang\b(?!\s+(?:\d|nay|truoc|sau|nam))", " ");
        // Do not silently substitute today for time phrases we cannot interpret safely.
        if (Regex.IsMatch(Regex.Replace(q, @"\b(moi|mot) thang\b|\b\d+\s+thang\b", ""), @"\b(nam\s*\d{4}|thang|hom qua|ngay mai|tuan truoc|tuan sau|nam ngoai|nam sau|ngay\s+\d)\b"))
            return new(null, null, "Vui lòng chọn ngày áp dụng cụ thể hoặc ghi ngày dd/MM/yyyy, tháng/năm trong câu hỏi.");
        return new(today, today, null);
    }
}
